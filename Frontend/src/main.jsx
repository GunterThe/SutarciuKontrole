import React from "react";
import ReactDOM from "react-dom/client";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import Login from "./Login";
import App from "./App";
import ArchivedRecords from "./ArchivedRecords"; // Import new component

ReactDOM.createRoot(document.getElementById("root")).render(
  <React.StrictMode>
    <Router>
      <Routes>
        <Route path="/" element={<Login />} />
        <Route path="/home" element={<App />} />
        <Route path="/archived" element={<ArchivedRecords />} /> {/* New route */}
      </Routes>
    </Router>
  </React.StrictMode>
);
