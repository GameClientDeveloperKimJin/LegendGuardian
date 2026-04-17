import { Outlet } from "react-router-dom";
import Sidebar from "./Sidebar";
import { Bell, UserCircle } from "lucide-react";

export default function Layout() {
  return (
    <div className="flex h-screen overflow-hidden">
      <Sidebar />
      <div className="flex-1 flex flex-col h-full overflow-y-auto px-6 py-4">
        {/* Top Header */}
        <header className="glass-panel w-full p-4 flex justify-between items-center mb-8">
          <div className="w-1/3">
            <input 
              type="text" 
              placeholder="Search..." 
              className="input-dark w-full"
            />
          </div>
          <div className="flex items-center gap-4 text-gray-300">
            <Bell size={20} className="cursor-pointer hover:text-white" />
            <div className="flex items-center gap-2 cursor-pointer bg-slate-800 px-3 py-1.5 rounded-full hover:bg-slate-700">
              <UserCircle size={24} className="text-blue-400" />
              <span className="text-sm font-medium">Admin User</span>
            </div>
          </div>
        </header>

        {/* Content Area */}
        <main className="flex-1 w-full max-w-6xl mx-auto">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
