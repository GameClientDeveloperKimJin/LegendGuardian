import { useState, useEffect } from "react";
import { collection, onSnapshot, addDoc, updateDoc, deleteDoc, doc } from "firebase/firestore";
import { db } from "../firebase/config";
import { MapPin, Target, Plus, Trash2, Route } from "lucide-react";

// 고정 구역
const FIXED_ZONES = [
  { id: "outdoor", name: "야외 광장 구역" },
  { id: "floor1", name: "건물 1층 구역" },
  { id: "floor2", name: "건물 2층 구역" },
  { id: "floor3", name: "건물 3층 구역" }
];

export default function RoutesPage() {
  const [missions, setMissions] = useState([]);
  const [routes, setRoutes] = useState([]);
  const [selectedRouteId, setSelectedRouteId] = useState("");
  const [newRouteName, setNewRouteName] = useState("");

  useEffect(() => {
    const unsubMissions = onSnapshot(collection(db, "missions"), (snapshot) => {
      const ms = [];
      snapshot.forEach(doc => ms.push({ id: doc.id, ...doc.data() }));
      setMissions(ms);
    });

    const unsubRoutes = onSnapshot(collection(db, "routes"), (snapshot) => {
      const rs = [];
      snapshot.forEach(doc => {
        const data = doc.data();
        if(data.zones) rs.push({ id: doc.id, ...data });
      });
      setRoutes(rs);
      
      // Select first route if none is selected
      if(rs.length > 0 && !selectedRouteId) {
        setSelectedRouteId(rs[0].id);
      }
    });

    return () => {
      unsubMissions();
      unsubRoutes();
    };
  }, [selectedRouteId]);

  const handleCreateRoute = async (e) => {
    e.preventDefault();
    if(!newRouteName.trim()) return;
    
    const docRef = await addDoc(collection(db, "routes"), {
      name: newRouteName,
      zones: { outdoor: [], floor1: [], floor2: [], floor3: [] },
      createdAt: new Date().toISOString()
    });
    setSelectedRouteId(docRef.id);
    setNewRouteName("");
  };

  const handleDeleteRoute = async (routeId) => {
    if(window.confirm("이 루트 세트를 정말 삭제하시겠습니까? (할당된 팀 구성에서 문제가 생길 수 있습니다)")) {
      await deleteDoc(doc(db, "routes", routeId));
      if(selectedRouteId === routeId) setSelectedRouteId(routes[0]?.id || "");
    }
  };

  const handleAddMissionToZone = async (zoneId, missionId) => {
    if (!missionId || !selectedRouteId) return;
    const currentRoute = routes.find(r => r.id === selectedRouteId);
    if (!currentRoute) return;

    const currentZoneMissions = currentRoute.zones[zoneId] || [];
    if (currentZoneMissions.includes(missionId)) return;

    const newZones = {
      ...currentRoute.zones,
      [zoneId]: [...currentZoneMissions, missionId]
    };

    await updateDoc(doc(db, "routes", selectedRouteId), {
      zones: newZones,
      updatedAt: new Date().toISOString()
    });
  };

  const handleRemoveMissionFromZone = async (zoneId, missionId) => {
    const currentRoute = routes.find(r => r.id === selectedRouteId);
    if (!currentRoute) return;

    const currentZoneMissions = currentRoute.zones[zoneId] || [];
    const newZones = {
      ...currentRoute.zones,
      [zoneId]: currentZoneMissions.filter(id => id !== missionId)
    };

    await updateDoc(doc(db, "routes", selectedRouteId), {
      zones: newZones,
      updatedAt: new Date().toISOString()
    });
  };

  const selectedRoute = routes.find(r => r.id === selectedRouteId);

  return (
    <div className="animate-fade-in pb-20">
      <h1 className="text-3xl font-bold mb-2">Routes Management</h1>
      <p className="text-gray-400 mb-8 max-w-2xl">
        팀에게 배정할 "코스(루트) 세트"를 만들고, 내부 구역별로 수행할 미션을 배치합니다.
      </p>

      <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
        
        {/* Left Side: Route Packages List */}
        <div className="lg:col-span-1 space-y-4">
          <div className="glass-panel p-5 border-blue-500/30 border-t-2">
            <h3 className="font-semibold text-white mb-4 flex items-center gap-2">
              <Plus size={18} className="text-blue-400" /> 새 코스 만들기
            </h3>
            <form onSubmit={handleCreateRoute} className="flex flex-col gap-3">
              <input 
                value={newRouteName}
                onChange={(e) => setNewRouteName(e.target.value)}
                placeholder="예: 초등부 코스 A"
                className="input-dark w-full text-sm"
              />
              <button type="submit" className="btn-blue text-sm w-full font-medium">생성하기</button>
            </form>
          </div>

          <div className="glass-panel p-0 overflow-hidden flex flex-col">
            <div className="p-4 bg-slate-800/50 border-b border-white/5 font-semibold text-white">등록된 코스 목록</div>
            <div className="flex flex-col divide-y divide-white/5 max-h-[500px] overflow-y-auto">
              {routes.map(route => (
                <div 
                  key={route.id} 
                  className={`p-4 flex items-center justify-between cursor-pointer transition ${selectedRouteId === route.id ? 'bg-blue-500/20 border-l-4 border-blue-500' : 'hover:bg-white/5 border-l-4 border-transparent'}`}
                  onClick={() => setSelectedRouteId(route.id)}
                >
                  <div className="flex items-center gap-2">
                    <Route size={16} className={selectedRouteId === route.id ? "text-blue-400" : "text-gray-500"} />
                    <span className={selectedRouteId === route.id ? "text-white font-medium" : "text-gray-400"}>{route.name}</span>
                  </div>
                  <button 
                    onClick={(e) => { e.stopPropagation(); handleDeleteRoute(route.id); }}
                    className="text-gray-500 hover:text-red-400 p-1"
                  >
                    <Trash2 size={14} />
                  </button>
                </div>
              ))}
              {routes.length === 0 && (
                <div className="p-8 text-center text-gray-500 text-sm">코스가 없습니다. 새로 만들어주세요.</div>
              )}
            </div>
          </div>
        </div>

        {/* Right Side: Zones Configuration */}
        <div className="lg:col-span-3">
          {!selectedRoute ? (
            <div className="glass-panel p-10 flex flex-col items-center justify-center text-gray-500 h-[600px] border-dashed">
              <Route size={48} className="text-gray-600 mb-4" />
              <p>좌측에서 루트 코스를 선택하거나 새로 생성해주세요.</p>
            </div>
          ) : (
            <div className="space-y-6">
              <div className="glass-panel p-5 bg-gradient-to-r from-blue-900/30 to-transparent">
                <h2 className="text-2xl font-bold text-white flex items-center gap-2">
                  <span className="text-blue-400">[{selectedRoute.name}]</span> 구역별 미션 설정
                </h2>
                <p className="text-gray-400 text-sm mt-1">이 코스에 배정된 팀이 각 구역에서 수행해야 할 미션들을 배치합니다.</p>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                {FIXED_ZONES.map(zone => {
                  const zoneMissions = selectedRoute.zones[zone.id] || [];
                  
                  return (
                    <div key={zone.id} className="glass-panel border-slate-700/50 flex flex-col h-[350px]">
                      <div className="p-3 bg-slate-800/50 border-b border-white/5 flex items-center justify-between">
                        <div className="flex items-center gap-2">
                          <MapPin size={18} className="text-rose-400" />
                          <h3 className="font-bold text-white">{zone.name}</h3>
                        </div>
                        <span className="text-xs bg-black/40 px-2 py-1 rounded text-gray-400">{zoneMissions.length}개 배치</span>
                      </div>

                      <div className="p-4 flex-1 flex flex-col gap-3">
                        <select
                          className="input-dark w-full text-sm appearance-none cursor-pointer text-gray-300 bg-black/50"
                          onChange={(e) => {
                            handleAddMissionToZone(zone.id, e.target.value);
                            e.target.value = ""; 
                          }}
                          defaultValue=""
                        >
                          <option value="" disabled>+ 미션 추가</option>
                          {missions.map(m => {
                            const isAssigned = zoneMissions.includes(m.id);
                            return (
                              <option key={m.id} value={m.id} disabled={isAssigned} className={isAssigned ? "text-gray-600" : "text-white"}>
                                {m.name} {isAssigned ? "(완료)" : ""}
                              </option>
                            )
                          })}
                        </select>

                        <div className="flex-1 overflow-y-auto space-y-2 custom-scrollbar">
                          {zoneMissions.map((mId, idx) => {
                            const mission = missions.find(m => m.id === mId);
                            if(!mission) return <div key={mId} className="hidden" />
                            
                            return (
                              <div key={mId} className="flex justify-between items-center p-2 border border-white/10 rounded hover:bg-white/5 group bg-black/20">
                                <div className="flex items-center gap-2 text-sm overflow-hidden">
                                  <span className="text-emerald-400 font-bold shrink-0">{idx + 1}.</span>
                                  <span className="text-gray-200 truncate">{mission.name}</span>
                                </div>
                                <button 
                                  onClick={() => handleRemoveMissionFromZone(zone.id, mId)}
                                  className="text-gray-500 hover:text-red-400 p-1 rounded opacity-0 group-hover:opacity-100 transition"
                                >
                                  <Trash2 size={14} />
                                </button>
                              </div>
                            )
                          })}
                          {zoneMissions.length === 0 && (
                            <div className="h-full flex items-center justify-center border border-dashed border-white/5 rounded text-gray-500 text-sm">
                              비어있음
                            </div>
                          )}
                        </div>
                      </div>
                    </div>
                  );
                })}
              </div>
            </div>
          )}
        </div>

      </div>
    </div>
  );
}
