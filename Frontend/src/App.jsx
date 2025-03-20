import React, { useState } from "react";
import { Container, Typography, Button, Box, Dialog, DialogTitle, DialogContent, DialogActions, TextField } from "@mui/material";
import { DataGrid } from "@mui/x-data-grid";

const App = () => {
  const [rows, setRows] = useState([
    { id: 1, name: "Sutartis", nr: 1, startdate: "2025-01-01", enddate: "2025-12-31", man: "Jonas Jonaitis", email: "a@a.lt", days: "1", freq: 2 }
  ]);

  const [open, setOpen] = useState(false);
  const [newRow, setNewRow] = useState({ name: "", age: "", email: "", status: "Active", role: "", location: "", department: "" });

  const handleArchive = (id) => {
    setRows(rows.filter(row => row.id !== id));
  };

  const handleEdit = (id) => {
    alert(`Edit row with ID: ${id}`); 
  };

  const handleAddRow = () => {
    setRows([...rows, { id: rows.length + 1, ...newRow }]);
    setNewRow({ name: "", age: "", email: "", status: "Active", role: "", location: "", department: "" });
    setOpen(false);
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
      <Typography variant="h4" gutterBottom sx={{ ml: -60}}>Sutarčių įrašai</Typography>
      <Button variant="contained" color="success" sx={{ mb: 2, ml: -60 }} onClick={() => setOpen(true)}>Pridėti naują įrašą</Button>
      
      <Box sx={{ height: 400, width: "190%", ml: -60}}>
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
        <DialogTitle>Pridėti naują įrašą</DialogTitle>
        <DialogContent>
          <TextField label="Sutarties Pavadinimas" fullWidth margin="dense" value={newRow.name} onChange={(e) => setNewRow({ ...newRow, name: e.target.value })} />
          <TextField label="DBSIS registracijos Nr." fullWidth margin="dense" value={newRow.nr} onChange={(e) => setNewRow({ ...newRow, age: e.target.value })} />
          <TextField label="Įsigaliojimo data" fullWidth margin="dense" type="date" value={newRow.startdate} InputLabelProps={{ shrink: true }} onChange={(e) => setNewRow({ ...newRow, email: e.target.value })} />
          <TextField label="Pabaigos data" fullWidth margin="dense" type="date" value={newRow.enddate} InputLabelProps={{ shrink: true }} onChange={(e) => setNewRow({ ...newRow, role: e.target.value })} />
          <TextField label="Atsakingas už sutarties vykdymą" fullWidth margin="dense" value={newRow.man} onChange={(e) => setNewRow({ ...newRow, location: e.target.value })} />
          <TextField label="Perspėti el. paštu  - adresas" fullWidth margin="dense" type="email" value={newRow.email} onChange={(e) => setNewRow({ ...newRow, department: e.target.value })} />
          <TextField label="Prieš kiek dienų iki pabaigos teikti priminimus" fullWidth margin="dense" type="number" value={newRow.days} onChange={(e) => setNewRow({ ...newRow, role: e.target.value })} />
          <TextField label="Kas kiek dienų siųsti priminimą" fullWidth margin="dense" type="number" value={newRow.freq} onChange={(e) => setNewRow({ ...newRow, location: e.target.value })} />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpen(false)}>Atšaukti</Button>
          <Button onClick={handleAddRow} variant="contained" color="primary">Pridėti</Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
};

export default App;