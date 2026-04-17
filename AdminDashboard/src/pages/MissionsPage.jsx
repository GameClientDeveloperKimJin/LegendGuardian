import { useState, useEffect } from "react";
import { collection, onSnapshot, addDoc, deleteDoc, doc } from "firebase/firestore";
import { db } from "../firebase/config";
import { ClipboardList, Plus, Trash2 } from "lucide-react";

export default function MissionsPage() {
  const [missions, setMissions] = useState([]);
  const [newMission, setNewMission] = useState({ name: "", reward: "" });
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    const unsubscribe = onSnapshot(collection(db, "missions"), (snapshot) => {
      const ms = [];
      snapshot.forEach(doc => ms.push({ id: doc.id, ...doc.data() }));
      setMissions(ms);
    });
    return () => unsubscribe();
  }, []);

  const handleAddMission = async (e) => {
    e.preventDefault();
    if (!newMission.name.trim() || !newMission.reward.trim()) return;
    
    setIsSubmitting(true);
    await addDoc(collection(db, "missions"), {
      name: newMission.name,
      reward: newMission.reward,
      createdAt: new Date().toISOString()
    });
    setNewMission({ name: "", reward: "" });
    setIsSubmitting(false);
  };

  const handleDelete = async (id) => {
    if(window.confirm("이 미션을 삭제하시겠습니까? (관련 루트에서는 보이지 않게 처리됩니다)")) {
      await deleteDoc(doc(db, "missions", id));
    }
  };

  return (
    <div className="animate-fade-in pb-20">
      <h1 className="text-3xl font-bold mb-2">Missions Management</h1>
      <p className="text-gray-400 mb-8 max-w-2xl">
        게임 내에서 사용될 개별 미션(임무)들을 생성하고 보상 내용을 정의합니다.
      </p>

      <div className="glass-panel p-6 mb-10 max-w-2xl">
        <h3 className="text-lg font-semibold mb-4 flex items-center gap-2">
          <Plus size={20} className="text-green-400" /> 새 미션 추가
        </h3>
        <form onSubmit={handleAddMission} className="flex flex-col gap-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm text-gray-400 mb-1">미션 이름</label>
              <input 
                type="text" 
                value={newMission.name}
                onChange={(e) => setNewMission({...newMission, name: e.target.value})}
                className="input-dark w-full"
                placeholder="예: 숨겨진 상자 찾기"
                required
              />
            </div>
            <div>
              <label className="block text-sm text-gray-400 mb-1">보상 내용</label>
              <input 
                type="text" 
                value={newMission.reward}
                onChange={(e) => setNewMission({...newMission, reward: e.target.value})}
                className="input-dark w-full"
                placeholder="예: 100 G / 단서A"
                required
              />
            </div>
          </div>
          <button 
            type="submit" 
            disabled={isSubmitting}
            className="btn-green mt-2 self-start"
          >
            미션 생성
          </button>
        </form>
      </div>

      <h2 className="text-2xl font-bold mb-5">생성된 미션 목록</h2>
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {missions.map(m => (
          <div key={m.id} className="glass-panel p-5 relative group">
            <div className="flex justify-between items-start mb-2">
              <div className="flex items-center gap-2 text-blue-300 font-bold text-lg">
                <ClipboardList size={20} />
                {m.name}
              </div>
              <button 
                onClick={() => handleDelete(m.id)} 
                className="text-red-500 hover:text-red-400 opacity-0 group-hover:opacity-100 transition"
              >
                <Trash2 size={18} />
              </button>
            </div>
            <div className="mt-4 p-3 bg-black/20 rounded border border-white/5">
              <span className="text-xs text-gray-500 block mb-1">보상</span>
              <span className="text-amber-400 font-medium">{m.reward}</span>
            </div>
            <div className="mt-2 text-xs text-gray-500">ID: {m.id}</div>
          </div>
        ))}
        {missions.length === 0 && (
          <div className="col-span-full text-center text-gray-500 py-10 glass-panel">
            아직 추가된 미션이 없습니다. 위 폼에서 새로 만들어주세요.
          </div>
        )}
      </div>
    </div>
  );
}
