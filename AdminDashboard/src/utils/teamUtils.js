import { collection, getDocs, writeBatch, doc } from "firebase/firestore";
import { db } from "../firebase/config";

// Fisher-Yates Shuffle
function shuffle(array) {
  const arr = [...array];
  for (let i = arr.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [arr[i], arr[j]] = [arr[j], arr[i]];
  }
  return arr;
}

export const assignRandomTeams = async () => {
  try {
    // 1. 모든 접속 유저(무소속 및 기존 팀 소속 등 전체 대상으로 하거나 특정 기준)를 가져옴
    // 여기서는 'users' 콜렉션의 모든 데이터를 가져온다고 가정
    const usersSnap = await getDocs(collection(db, "users"));
    if(usersSnap.empty) {
        alert("유저가 존재하지 않습니다.");
        return;
    }
    
    let allUsers = [];
    usersSnap.forEach(doc => {
        allUsers.push({ id: doc.id, ...doc.data() });
    });

    // 2. 랜덤 셔플
    allUsers = shuffle(allUsers);

    // 3. 5인 기준으로 팀 나누기 로직 (꽉 찬 5인 팀들을 만들고, 나머지는 마지막 팀으로)
    const n = allUsers.length;
    
    // 팀별 인원 할당 배열 (예: 7명이면 [5, 2], 12명이면 [5, 5, 2], 3명이면 [3])
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

    // 4. 유저를 팀에 분배
    const distributedTeams = [];
    let currentIndex = 0;
    
    for (let i = 0; i < teamCapacities.length; i++) {
        const capacity = teamCapacities[i];
        const teamMembers = allUsers.slice(currentIndex, currentIndex + capacity);
        distributedTeams.push({
            name: `Team ${i + 1}`,
            members: teamMembers,
            score: 0,
            routeId: null // 추후 할당될 루트 아이디
        });
        currentIndex += capacity;
    }

    // 5. Firestore에 일괄 업데이트 (기존 팀 데이터가 있다면 초기화 필요 가능)
    const batch = writeBatch(db);
    
    // 이전에 있던 팀들 삭제 (옵션, 전체 초기화용. 기존 팀 ID를 안다면 삭제)
    const oldTeamsSnap = await getDocs(collection(db, "teams"));
    oldTeamsSnap.forEach(doc => batch.delete(doc.ref));

    // 새로운 팀 생성
    distributedTeams.forEach((team) => {
        const teamRef = doc(collection(db, "teams"));
        // 팀원이 아닌 유저 레퍼런스나 ID 목록만 저장하고 UI에서 연동하도록 구성
        const memberIds = team.members.map(m => m.id);
        batch.set(teamRef, {
            name: team.name,
            members: memberIds,
            score: team.score,
            createdAt: new Date().toISOString()
        });
        
        // 유저 문서에도 팀 ID 기입 (양방향 참조)
        team.members.forEach((m) => {
            const userRef = doc(db, "users", m.id);
            batch.update(userRef, { teamId: teamRef.id });
        });
    });

    await batch.commit();
    alert("랜덤 5인 1조 편성이 성공적으로 완료되었습니다!");

  } catch (err) {
    console.error("팀 분배 오류:", err);
    alert("팀 생성 중 서버 오류가 발생했습니다.");
  }
}
