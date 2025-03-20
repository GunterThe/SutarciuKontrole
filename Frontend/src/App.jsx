import React, { useState } from "react";
import { Container, Typography, Button, Box, Dialog, DialogTitle, DialogContent, DialogActions, TextField } from "@mui/material";
import { DataGrid } from "@mui/x-data-grid";
import { useNavigate } from "react-router-dom";

const App = () => {
  const navigate = useNavigate();
  const [rows, setRows] = useState([
    { id: 1, name: "Sutartis", nr: 1, startdate: "2025-01-01", enddate: "2025-12-31", man: "Jonas Jonaitis", email: "a@a.lt", days: "1", freq: 2 }
  ]);

  const [open, setOpen] = useState(false);
  const [selectedRow, setSelectedRow] = useState(null); 
  const [newRow, setNewRow] = useState({
    name: "", nr: "", startdate: "", enddate: "", man: "", email: "", days: "", freq: ""
  });

  const handleArchive = (id) => {
    const rowToArchive = rows.find(row => row.id === id);
    const updatedRows = rows.filter(row => row.id !== id);
    
    setRows(updatedRows);
  
    const archivedRecords = JSON.parse(localStorage.getItem("archivedRecords")) || [];
    localStorage.setItem("archivedRecords", JSON.stringify([...archivedRecords, rowToArchive]));
  };
  

  const handleEdit = (id) => {
    const rowToEdit = rows.find(row => row.id === id);
    setSelectedRow(id);
    setNewRow(rowToEdit); 
    setOpen(true);
  };

  const handleSaveRow = () => {
    if (selectedRow !== null) {
      setRows(rows.map(row => (row.id === selectedRow ? { ...newRow, id: selectedRow } : row)));
    } else {
      setRows([...rows, { id: rows.length + 1, ...newRow }]);
    }
    setOpen(false);
    setSelectedRow(null);
    setNewRow({ name: "", nr: "", startdate: "", enddate: "", man: "", email: "", days: "", freq: "" });
  };

  const columns = [
    { field: "name", headerName: "Sutarties Pavadinimas", flex: 2 },
    { field: "nr", headerName: "DBSIS registracijos Nr.", flex: 2 },
    { field: "startdate", headerName: "Įsigaliojimo data", flex: 2 },
    { field: "enddate", headerName: "Pabaigos data", flex: 2 },
    { field: "man", headerName: "Atsakingas už sutarties vykdymą", flex: 2 },
    { field: "email", headerName: "Perspėti el. paštu  - adresas", flex: 2 },
    { field: "days", headerName: "Prieš kiek dienų iki pabaigos teikti priminimus", flex: 2 },
    { field: "freq", headerName: "Kas kiek dienų siųsti priminimą", flex: 2 },
    {
      field: "actions",
      headerName: "Veiksmai",
      flex: 3,
      renderCell: (params) => (
        <>
          <Button variant="contained" color="primary" size="small" onClick={() => handleEdit(params.row.id)}>Redaguoti</Button>
          <Button variant="contained" color="error" size="small" onClick={() => handleArchive(params.row.id)} sx={{ ml: 1 }}>Archyvuoti</Button>
        </>
      )
    }
  ];

  return (
    <Container maxWidth="lg" sx={{ mt: 7 }}>
      <Button 
      variant="contained" 
      color="error" 
      sx={{ position: "absolute", top: 20, right: 20 }}
      onClick={() => navigate("/")}
      >
      Atsijungti
      </Button>
      <Typography variant="h4" gutterBottom sx={{ ml: -60}}>Sutarčių įrašai</Typography>
      <Button variant="contained" color="success" sx={{ mb: 2, ml: -60 }} onClick={() => { setOpen(true); setSelectedRow(null); 
      setNewRow({ name: "", nr: "", startdate: "", enddate: "", man: "", email: "", days: "", freq: "" });
  }}
>
  Pridėti naują įrašą
</Button>
<Button variant="contained" color="secondary" sx={{ mb: 2, ml: 2 }} onClick={() => navigate("/archived")}>
  Archyvuoti įrašai
</Button>
      <Box sx={{ height: 400, width: "190%", ml: -60 }}>
        <DataGrid rows={rows} columns={columns} pageSize={7} 
          localeText={{
            noRowsLabel: "Nėra duomenų",
            toolbarDensity: "Eilutės per puslapį",
            MuiTablePagination: {
              labelRowsPerPage: "Eilučių per puslapį",
            }
          }} 
        />
      </Box>
      <Dialog open={open} onClose={() => setOpen(false)}>
        <DialogTitle>{selectedRow !== null ? "Redaguoti įrašą" : "Pridėti naują įrašą"}</DialogTitle>
        <DialogContent>
          <TextField label="Sutarties Pavadinimas" fullWidth margin="dense" value={newRow.name} onChange={(e) => setNewRow({ ...newRow, name: e.target.value })} />
          <TextField label="DBSIS registracijos Nr." fullWidth margin="dense" value={newRow.nr} onChange={(e) => setNewRow({ ...newRow, nr: e.target.value })} />
          <TextField label="Įsigaliojimo data" fullWidth margin="dense" type="date" value={newRow.startdate} InputLabelProps={{ shrink: true }} onChange={(e) => setNewRow({ ...newRow, startdate: e.target.value })} />
          <TextField label="Pabaigos data" fullWidth margin="dense" type="date" value={newRow.enddate} InputLabelProps={{ shrink: true }} onChange={(e) => setNewRow({ ...newRow, enddate: e.target.value })} />
          <TextField label="Atsakingas už sutarties vykdymą" fullWidth margin="dense" value={newRow.man} onChange={(e) => setNewRow({ ...newRow, man: e.target.value })} />
          <TextField label="Perspėti el. paštu  - adresas" fullWidth margin="dense" type="email" value={newRow.email} onChange={(e) => setNewRow({ ...newRow, email: e.target.value })} />
          <TextField label="Prieš kiek dienų iki pabaigos teikti priminimus" fullWidth margin="dense" type="number" value={newRow.days} onChange={(e) => setNewRow({ ...newRow, days: e.target.value })} />
          <TextField label="Kas kiek dienų siųsti priminimą" fullWidth margin="dense" type="number" value={newRow.freq} onChange={(e) => setNewRow({ ...newRow, freq: e.target.value })} />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpen(false)}>Atšaukti</Button>
          <Button onClick={handleSaveRow} variant="contained" color="primary">
            {selectedRow !== null ? "Išsaugoti" : "Pridėti"}
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
};

export default App;