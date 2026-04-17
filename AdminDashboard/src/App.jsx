import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import Layout from "./components/Layout";
import DashboardPage from "./pages/DashboardPage";
import TeamsPage from "./pages/TeamsPage";
import UsersPage from "./pages/UsersPage";
import MissionsPage from "./pages/MissionsPage";
import RoutesPage from "./pages/RoutesPage";

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<Layout />}>
          <Route index element={<DashboardPage />} />
          <Route path="teams" element={<TeamsPage />} />
          <Route path="users" element={<UsersPage />} />
          <Route path="missions" element={<MissionsPage />} />
          <Route path="routes" element={<RoutesPage />} />
          <Route path="*" element={<div className="p-10 text-gray-400">Working on it... (Comin soon)</div>} />
        </Route>
      </Routes>
    </Router>
  );
}

export default App;
