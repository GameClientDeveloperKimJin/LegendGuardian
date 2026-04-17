import { useState, useEffect } from "react";
import { collection, onSnapshot, deleteDoc, doc } from "firebase/firestore";
import { db } from "../firebase/config";
import { UsersRound, Trash2 } from "lucide-react";

export default function UsersPage() {
  const [users, setUsers] = useState([]);
  const [teams, setTeams] = useState([]);

  useEffect(() => {
    const unsubscribeUsers = onSnapshot(collection(db, "users"), (snapshot) => {
      const usersData = [];
      snapshot.forEach(doc => usersData.push({ id: doc.id, ...doc.data() }));
      setUsers(usersData);
    });

    const unsubscribeTeams = onSnapshot(collection(db, "teams"), (snapshot) => {
      const teamsData = [];
      snapshot.forEach(doc => teamsData.push({ id: doc.id, ...doc.data() }));
      setTeams(teamsData);
    });

    return () => {
      unsubscribeUsers();
      unsubscribeTeams();
    };
  }, []);

  const handleDeleteUser = async (userId) => {
    if(window.confirm("정말 이 유저를 삭제하시겠습니까?")) {
      await deleteDoc(doc(db, "users", userId));
    }
  };

  return (
    <div className="animate-fade-in pb-20">
      <h1 className="text-3xl font-bold mb-2">Users Management</h1>
      <p className="text-gray-400 mb-8 max-w-2xl">
        현재 앱/게임에 연동된 모든 유저를 확인하고 관리합니다.
      </p>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {users.map(user => {
          const team = teams.find(t => t.id === user.teamId);
          return (
            <div key={user.id} className="glass-panel p-5 relative overflow-hidden group">
              <div className="flex items-center gap-3 mb-3">
                <div className="p-3 bg-blue-500/20 rounded-full">
                  <UsersRound size={24} className="text-blue-400" />
                </div>
                <div>
                  <h3 className="font-bold text-lg text-white">{user.name || "Unknown User"}</h3>
                  <p className="text-sm text-gray-400">ID: {user.id}</p>
                </div>
              </div>
              <div className="mt-4 pt-4 border-t border-white/10 flex justify-between items-center text-sm">
                <div className="px-3 py-1 rounded-full bg-slate-800 border border-slate-700">
                  <span className="text-gray-400">Team: </span>
                  <span className="font-semibold text-blue-300">{team ? team.name : "무소속"}</span>
                </div>
                <button 
                  onClick={() => handleDeleteUser(user.id)}
                  className="opacity-0 group-hover:opacity-100 transition p-2 text-red-500 hover:bg-red-500/20 rounded-md"
                  title="유저 삭제"
                >
                  <Trash2 size={18} />
                </button>
              </div>
            </div>
          );
        })}
        {users.length === 0 && (
          <div className="col-span-full glass-panel p-10 text-center text-gray-500">
            데이터베이스에 유저가 없습니다. 앱/게임에서 로그인하여 연동해주세요.
          </div>
        )}
      </div>
    </div>
  );
}
