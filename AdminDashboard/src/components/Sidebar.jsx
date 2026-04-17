import { NavLink } from "react-router-dom";
import { LayoutDashboard, Map, Users, UsersRound, ClipboardList, SendToBack, ShieldCheck } from "lucide-react";

export default function Sidebar() {
  const navItems = [
    { name: "Dashboard", path: "/", icon: <LayoutDashboard size={20} /> },
    { name: "Routes", path: "/routes", icon: <Map size={20} /> },
    { name: "Teams", path: "/teams", icon: <Users size={20} /> },
    { name: "Users", path: "/users", icon: <UsersRound size={20} /> },
    { name: "Leg. Missions", path: "/missions", icon: <ClipboardList size={20} /> },
    { name: "Submissions", path: "/submissions", icon: <SendToBack size={20} /> }
  ];

  return (
    <aside className="w-64 glass-panel h-screen m-4 hidden md:flex flex-col p-4">
      <div className="flex items-center gap-3 mb-10 pl-2">
        <ShieldCheck size={28} className="text-blue-500" />
        <h1 className="text-xl font-bold bg-clip-text text-transparent bg-gradient-to-r from-blue-400 to-blue-600">
          Admin Panel
        </h1>
      </div>
      
      <nav className="flex flex-col gap-2">
        {navItems.map((item) => (
          <NavLink
            key={item.name}
            to={item.path}
            className={({ isActive }) => 
              `sidebar-item ${isActive ? "active font-medium" : ""}`
            }
          >
            {item.icon}
            <span>{item.name}</span>
          </NavLink>
        ))}
      </nav>
    </aside>
  );
}
