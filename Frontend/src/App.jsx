import React from "react";
import { useNavigate } from "react-router-dom";

const App = () => {
  const navigate = useNavigate();

  const handleLogout = () => {
    navigate("/");
  };

  return (
    <div>
      <h1>Pagrindinis puslapis</h1>
      <button onClick={handleLogout}>Atsijungti</button>
    </div>
  );
};

export default App;
