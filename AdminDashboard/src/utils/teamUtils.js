import { collection, getDocs, writeBatch, doc } from "firebase/firestore";
import { ref, set, update } from "firebase/database";
import { db, rtdb } from "../firebase/config";

// Fisher-Yates Shuffle
function shuffle(array) {
  const arr = [...array];
  for (let i = arr.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [arr[i], arr[j]] = [arr[j], arr[i]];
  }
  return arr;
}

// 1. 임시 랜덤 매칭 로직 (DB 저장 안 함)
export const generateRandomTeams = async () => {
  try {
    const usersSnap = await getDocs(collection(db, "users"));
    if(usersSnap.empty) {
        throw new Error("유저가 존재하지 않습니다.");
    }
    
    let allUsers = [];
    usersSnap.forEach(doc => {
        allUsers.push({ id: doc.id, ...doc.data() });
    });

    allUsers = shuffle(allUsers);

    const n = allUsers.length;
    let teamCapacities = [];
    let remaining = n;
    
    while(remaining > 0) {
        if(remaining >= 5) {
            teamCapacities.push(5);
            remaining -= 5;
        } else {
            teamCapacities.push(remaining);
            remaining = 0;
        }
    }

    const distributedTeams = [];
    let currentIndex = 0;
    
    // A, B, C 알파벳 배열 (기본적으로 26개 지원, 필요 이상의 경우 AA 등의 로직은 현재 생략)
    const alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    for (let i = 0; i < teamCapacities.length; i++) {
        const capacity = teamCapacities[i];
        const teamMembers = allUsers.slice(currentIndex, currentIndex + capacity);
        const teamAlias = alphabet[i] || `T${i+1}`;
        distributedTeams.push({
            name: `${teamAlias}팀`,
            systemTeamId: teamAlias, // A, B, C 등 실제 유저 문서에 들어갈 값
            members: teamMembers,
            score: 0,
            routeId: null
        });
        currentIndex += capacity;
    }

    return distributedTeams;
  } catch (err) {
    console.error("팀 분배 오류:", err);
    throw err;
  }
}

// 2. 확정 로직 (Firestore 문서 변경 + RTDB 신호 전송)
export const commitTeamsToDB = async (distributedTeams) => {
  try {
    const batch = writeBatch(db);
    
    // 기존 팀들 삭제 초기화
    const oldTeamsSnap = await getDocs(collection(db, "teams"));
    oldTeamsSnap.forEach(doc => batch.delete(doc.ref));

    distributedTeams.forEach((team) => {
        const teamRef = doc(collection(db, "teams"));
        const memberIds = team.members.map(m => m.id);
        
        batch.set(teamRef, {
            name: team.name,
            members: memberIds,
            score: team.score,
            createdAt: new Date().toISOString()
        });
        
        // 유저 문서에 팀 ID 기입 (알파벳 ID 저장)
        team.members.forEach((m) => {
            const userRef = doc(db, "users", m.id);
            batch.update(userRef, { teamId: team.systemTeamId });
        });
    });

    // Firestore 트랜잭션 수행
    await batch.commit();

    // 완료 후 Realtime Database에 팀 매칭 완료 신호(Broadcast)
    const statusRef = ref(rtdb, "gameStatus/teamReady");
    await set(statusRef, {
        isReady: true,
        timestamp: Date.now()
    });

    // 기존 설계 문서(TeamReady_Implementation.md)에 따라 teams 하위 각 팀의 상태도 ready로 갱신 (유저 편의 및 호환용)
    const updates = {};
    distributedTeams.forEach(team => {
      const playersObj = {};
      team.members.forEach(m => { playersObj[m.id] = true; });
      updates[`teams/${team.systemTeamId}/status`] = "ready";
      updates[`teams/${team.systemTeamId}/players`] = playersObj;
    });
    
    // update를 사용하면 최상위에서 여러 경로를 동시 수정 가능
    await update(ref(rtdb), updates);

  } catch (err) {
    console.error("팀 확정 오류:", err);
    throw err;
  }
}
