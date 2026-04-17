import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import Layout from "./components/Layout";
import TeamsPage from "./pages/TeamsPage";

// 더미 Dashboard 페이지
function Dashboard() {
  return <div className="p-10"><h1 className="text-3xl font-bold text-gray-300">Dashboard UI</h1></div>;
}

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<Layout />}>
          <Route index element={<Dashboard />} />
          <Route path="teams" element={<TeamsPage />} />
          {/* 다른 페이지들도 추후 추가 가능 */}
          <Route path="*" element={<div className="p-10">Working on it...</div>} />
        </Route>
      </Routes>
    </Router>
  );
}

export default App;
