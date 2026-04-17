import { useState, useEffect } from "react";
import { collection, onSnapshot } from "firebase/firestore";
import { db } from "../firebase/config";
import { UsersRound, Users, ClipboardList, Map } from "lucide-react";

export default function DashboardPage() {
  const [stats, setStats] = useState({ users: 0, teams: 0, missions: 0, routes: 0 });

  useEffect(() => {
    const unsubUsers = onSnapshot(collection(db, "users"), snap => setStats(s => ({ ...s, users: snap.size })));
    const unsubTeams = onSnapshot(collection(db, "teams"), snap => setStats(s => ({ ...s, teams: snap.size })));
    const unsubMissions = onSnapshot(collection(db, "missions"), snap => setStats(s => ({ ...s, missions: snap.size })));
    const unsubRoutes = onSnapshot(collection(db, "routes"), snap => setStats(s => ({ ...s, routes: snap.size })));

    return () => {
      unsubUsers(); unsubTeams(); unsubMissions(); unsubRoutes();
    };
  }, []);

  const statCards = [
    { title: "Total Users", count: stats.users, icon: <UsersRound size={32} className="text-blue-400" />, color: "border-blue-500/30" },
    { title: "Total Teams", count: stats.teams, icon: <Users size={32} className="text-emerald-400" />, color: "border-emerald-500/30" },
    { title: "Total Missions", count: stats.missions, icon: <ClipboardList size={32} className="text-amber-400" />, color: "border-amber-500/30" },
    { title: "Total Routes", count: stats.routes, icon: <Map size={32} className="text-rose-400" />, color: "border-rose-500/30" },
  ];

  return (
    <div className="animate-fade-in pb-20">
      <h1 className="text-3xl font-bold mb-2">Dashboard Overview</h1>
      <p className="text-gray-400 mb-8 max-w-2xl">
        전체 게임(앱)의 실시간 상태와 데이터 통계를 한눈에 확인합니다.
      </p>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {statCards.map((card, idx) => (
          <div key={idx} className={`glass-panel p-6 border-t-4 ${card.color} flex items-center justify-between`}>
            <div>
              <p className="text-sm text-gray-400 font-medium mb-1">{card.title}</p>
              <h3 className="text-4xl font-bold text-white">{card.count}</h3>
            </div>
            <div className="p-4 bg-white/5 rounded-full">
              {card.icon}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
