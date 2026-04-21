import { useState, useEffect } from "react";
import { collection, onSnapshot, addDoc, doc, deleteDoc, updateDoc } from "firebase/firestore";
import { db } from "../firebase/config";
import { generateRandomTeams, commitTeamsToDB } from "../utils/teamUtils";
import { Users, Shuffle, Plus, UserPlus, Trash2, CheckCircle } from "lucide-react";

export default function TeamsPage() {
  const [teams, setTeams] = useState([]);
  const [users, setUsers] = useState([]);
  const [routes, setRoutes] = useState([]);
  const [newTeamName, setNewTeamName] = useState("");
  const [selectedUser, setSelectedUser] = useState("");
  const [selectedTeam, setSelectedTeam] = useState("");
  const [isProcessing, setIsProcessing] = useState(false);
  const [tempTeams, setTempTeams] = useState(null);

  useEffect(() => {
    // 실시간 팀 데이터 구독
    const unsubscribeTeams = onSnapshot(collection(db, "teams"), (snapshot) => {
      const teamsData = [];
      snapshot.forEach(doc => teamsData.push({ id: doc.id, ...doc.data() }));
      setTeams(teamsData);
    });

    // 실시간 유저 데이터 구독
    const unsubscribeUsers = onSnapshot(collection(db, "users"), (snapshot) => {
      const usersData = [];
      snapshot.forEach(doc => usersData.push({ id: doc.id, ...doc.data() }));
      setUsers(usersData);
    });

    // 실시간 루트(코스) 데이터 구독
    const unsubscribeRoutes = onSnapshot(collection(db, "routes"), (snapshot) => {
      const routesData = [];
      snapshot.forEach(doc => routesData.push({ id: doc.id, name: doc.data().name }));
      setRoutes(routesData);
    });

    return () => {
      unsubscribeTeams();
      unsubscribeUsers();
      unsubscribeRoutes();
    };
  }, []);

  const handleRandomShuffle = async () => {
    setIsProcessing(true);
    try {
      const result = await generateRandomTeams();
      setTempTeams(result);
    } catch(err) {
      alert("팀 생성 중 오류가 발생했습니다.");
    }
    setIsProcessing(false);
  };

  const handleConfirmTeams = async () => {
    if(!tempTeams) return;
    if(!window.confirm("현재 보이는 구성대로 팀 편성을 확정하고 클라이언트에 시작 신호를 보내시겠습니까?")) return;
    setIsProcessing(true);
    try {
      await commitTeamsToDB(tempTeams);
      setTempTeams(null);
      alert("팀 편성이 성공적으로 확정되었습니다!");
    } catch(err) {
      alert("팀 저장 중 오류가 발생했습니다.");
    }
    setIsProcessing(false);
  };

  const handleCreateTeam = async () => {
    if(!newTeamName.trim()) return;
    await addDoc(collection(db, "teams"), {
      name: newTeamName,
      members: [],
      score: 0,
      createdAt: new Date().toISOString()
    });
    setNewTeamName("");
  };

  const handleAssignUser = async () => {
    if(!selectedUser || !selectedTeam) return;
    
    // 타겟 팀 찾기
    const team = teams.find(t => t.id === selectedTeam);
    if(team) {
      const newMembers = [...(team.members || []), selectedUser];
      const teamRef = doc(db, "teams", selectedTeam);
      await updateDoc(teamRef, { members: newMembers });
      
      const userRef = doc(db, "users", selectedUser);
      await updateDoc(userRef, { teamId: selectedTeam });
      
      setSelectedUser("");
      setSelectedTeam("");
    }
  };

  const handleDeleteTeam = async (teamId) => {
    if(window.confirm("정말 이 팀을 삭제하시겠습니까?")) {
      await deleteDoc(doc(db, "teams", teamId));
    }
  };

  return (
    <div className="animate-fade-in pb-20">
      <div className="flex justify-between items-start mb-2">
        <div>
          <h1 className="text-3xl font-bold mb-2">Teams Management</h1>
          <p className="text-gray-400 max-w-2xl">
            팀을 수동으로 구성하거나 랜덤 자동 알고리즘으로 5인 1조 편성을 진행합니다.
          </p>
        </div>
        <button 
          onClick={handleRandomShuffle}
          disabled={isProcessing}
          className="btn-green text-sm flex gap-2 items-center"
        >
          <Shuffle size={18} />
          {isProcessing ? "처리 중..." : "랜덤 5인 1조 편성 (All Players)"}
        </button>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mt-8 mb-10">
        {/* 수동 팀 만들기 */}
        <div className="glass-panel p-6 border-slate-700/50">
          <h3 className="text-lg font-semibold mb-4 flex items-center gap-2">
            <Plus size={20} /> 수동 팀 만들기
          </h3>
          <div className="flex gap-3">
            <input 
              type="text" 
              value={newTeamName}
              onChange={(e) => setNewTeamName(e.target.value)}
              placeholder="새 팀 이름 (예: A팀)" 
              className="input-dark flex-1 py-2 px-3 bg-slate-800/50"
            />
            <button onClick={handleCreateTeam} className="btn-blue bg-indigo-500 hover:bg-indigo-600 px-6 py-2 rounded-md font-medium text-white shadow-md">
              생 성
            </button>
          </div>
        </div>

        {/* 수동 배정 */}
        <div className="glass-panel p-6 border-slate-700/50">
          <div className="flex items-center gap-3 mb-4">
            <h3 className="text-lg font-semibold flex items-center gap-2">
              <UserPlus size={20} /> 무소속 유저 수동 배정
            </h3>
          </div>
          <div className="flex flex-col gap-3">
            <div className="flex gap-3">
              <select 
                value={selectedUser}
                onChange={(e) => setSelectedUser(e.target.value)}
                className="input-dark flex-1 py-2 px-3 appearance-none bg-slate-800/50 text-gray-300"
              >
                <option value="">-- 유저 선택 --</option>
                {users.filter(u => !u.teamId).map(u => (
                  <option key={u.id} value={u.id}>{u.name || u.id}</option>
                ))}
              </select>
              <select 
                value={selectedTeam}
                onChange={(e) => setSelectedTeam(e.target.value)}
                className="input-dark flex-1 py-2 px-3 appearance-none bg-slate-800/50 text-gray-300"
              >
                <option value="">-- 팀 선택 --</option>
                {teams.map(t => (
                  <option key={t.id} value={t.id}>{t.name}</option>
                ))}
              </select>
              <button onClick={handleAssignUser} className="btn-blue bg-indigo-500 hover:bg-indigo-600 px-6 py-2 rounded-md font-medium text-white shadow-md">
                팀원 배정
              </button>
            </div>
          </div>
        </div>
      </div>

      {/* 임시 배정 결과 뷰 */}
      {tempTeams && (
        <div className="mb-10 bg-indigo-900/40 border border-indigo-500/50 rounded-xl p-6">
          <div className="flex flex-col md:flex-row justify-between items-start md:items-center mb-6 gap-4">
            <h2 className="text-2xl font-bold text-indigo-200">배정 대기열 (임시 결과)</h2>
            <div className="flex flex-wrap gap-3">
              <button onClick={handleRandomShuffle} disabled={isProcessing} className="btn border border-indigo-400 text-indigo-300 hover:bg-indigo-500/20 py-2 px-4 rounded-lg text-sm flex items-center gap-2">
                <Shuffle size={16} /> 팀 재구성 (다시 돌리기)
              </button>
              <button onClick={handleConfirmTeams} disabled={isProcessing} className="btn bg-green-600 hover:bg-green-500 text-white rounded-lg px-5 py-2 font-bold flex items-center gap-2">
                <CheckCircle size={18} /> 팀 구성 확정 (게임 시작)
              </button>
            </div>
          </div>
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 md:grid-cols-3 gap-4">
            {tempTeams.map((team, idx) => (
              <div key={idx} className="bg-black/40 p-4 rounded-lg border border-white/10">
                <h4 className="text-lg font-bold text-blue-300 mb-2">{team.name} ({team.members.length}명)</h4>
                <ul className="space-y-1">
                  {team.members.map(m => (
                    <li key={m.id} className="text-gray-300 text-sm flex items-center gap-2">
                      <span className="w-1.5 h-1.5 rounded-full bg-indigo-500"></span>{m.name || m.id}
                    </li>
                  ))}
                </ul>
              </div>
            ))}
          </div>
        </div>
      )}

      <h2 className="text-2xl font-bold mb-5 mt-10">현재 팀 목록</h2>
      <div className="space-y-4">
        {teams.length === 0 ? (
          <div className="text-gray-500 text-center p-10 glass-panel">생성된 팀이 없습니다.</div>
        ) : (
          teams.map(team => (
            <div key={team.id} className="glass-panel p-6 border-slate-700/50 relative overflow-hidden">
              <div className="flex flex-col md:flex-row justify-between items-start md:items-center border-b border-white/10 pb-4 mb-4 gap-4">
                <div className="flex items-center gap-3">
                  <Users size={24} className="text-blue-400" />
                  <h3 className="text-xl font-bold text-blue-100">{team.name}</h3>
                </div>
                <div className="flex items-center gap-3">
                  {/* 코스 배정 */}
                  <select
                    className="input-dark text-sm bg-black/40 text-gray-300"
                    value={team.routeId || ""}
                    onChange={(e) => updateDoc(doc(db, "teams", team.id), { routeId: e.target.value })}
                  >
                    <option value="">-- 수행할 코스 미배치 --</option>
                    {routes.map(r => (
                      <option key={r.id} value={r.id}>{r.name}</option>
                    ))}
                  </select>
                  <button onClick={() => handleDeleteTeam(team.id)} className="p-2 bg-red-500/20 text-red-400 rounded-md hover:bg-red-500/40 transition">
                    <Trash2 size={20} />
                  </button>
                </div>
              </div>
              <p className="text-green-400 font-medium mb-4">팀 점수 합계: {team.score || 0} pts</p>
              
              <div className="flex flex-col gap-2">
                {team.members && team.members.length > 0 ? (
                  team.members.map(memberId => {
                    const user = users.find(u => u.id === memberId);
                    return (
                      <div key={memberId} className="flex justify-between items-center py-2 border-b border-white/5 last:border-0">
                        <span className="text-gray-300">{user ? (user.name || "Unknown") : "Unknown"}</span>
                        <span className="text-gray-500 text-sm">0 pts</span>
                      </div>
                    );
                  })
                ) : (
                  <div className="text-gray-500 text-sm py-2">팀원이 없습니다.</div>
                )}
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
}
