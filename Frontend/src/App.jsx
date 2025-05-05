import React, { useState, useEffect } from "react";
import { Container, Typography, Button, Box, Dialog, DialogTitle, DialogContent, DialogActions, TextField, Autocomplete } from "@mui/material";
import { DataGrid } from "@mui/x-data-grid";
import { useNavigate } from "react-router-dom";
import { jwtDecode } from "jwt-decode";
import { Irasas } from "./models/Irasas"
import { getIrasasById, getIrasasNaudotojai, createIrasas, getAllNaudotojai, updateIrasas, archiveIrasas } from "./api"; // Import the API functions

const App = () => {
  const token = localStorage.getItem("token");
  const id = token ? jwtDecode(token).sub : null; // Decode the user ID from the token
  console.log(id);

  const navigate = useNavigate();
  const [rows, setRows] = useState([]); // Initialize rows as an empty array
  const [open, setOpen] = useState(false);
  const [selectedRow, setSelectedRow] = useState(null);
  const [availableCustomers, setAvailableCustomers] = useState([]); // Store all Naudotojai for the Autocomplete
  const [newRow, setNewRow] = useState({
    name: "", nr: "", startdate: "", enddate: "", man: "", email: "", days: "", freq: "", customers: []
  });

  useEffect(() => {
    // Fetch Irasai for the user when the component mounts
    const fetchIrasai = async () => {
      try {
        const response = await getIrasasById(id, false); // Fetch non-archived Irasai for the user
        const irasai = response.$values || [];

        // Fetch Prekes_Adminas for each Irasas
        const irasaiWithAdmins = await Promise.all(
          irasai.map(async (irasas) => {
            const names = [];
            console.log("Iraso info: ", irasas.irasas);
            const naudotojai = await getIrasasNaudotojai(irasas.irasas.id); // Fetch Naudotojai for the Irasas
            naudotojai.$values.map((naudotojas) => {
              names.push(naudotojas.vardas + " " + naudotojas.pavarde + " " + naudotojas.pareigos); 
            }
            )
            console.log(names);
            return { id: irasas.irasas.id, name: irasas.irasas.pavadinimas, nr: irasas.irasas.id_dokumento, startdate: irasas.irasas.isigaliojimo_data, 
              enddate: irasas.irasas.pabaigos_data, man: " ", email: irasas.irasas.pastas_kreiptis,
              days: irasas.irasas.dienos_pries, freq: irasas.irasas.dienu_daznumas, prekesAdminas: names }; // Add Prekes_Adminas to the Irasas
          })
        );


        setRows(irasaiWithAdmins); // Set the fetched Irasai with Prekes_Adminas as rows
      } catch (error) {
        console.error("Error fetching Irasai or Prekes_Adminas:", error);
      }
    };

    // Fetch all Naudotojai for the Autocomplete
    const fetchNaudotojai = async () => {
      try {
        const response2 = await getAllNaudotojai();
        const naudotojai = response2.$values || [];
        setAvailableCustomers(naudotojai); // Set the fetched Naudotojai for the Autocomplete
      } catch (error) {
        console.error("Error fetching Naudotojai:", error);
      }
    };

    if (id) {
      fetchIrasai();
      fetchNaudotojai();
    }
  }, [id]);

  const handleArchive = async (id) => {
    try {
      await archiveIrasas(id);
      
      window.location.reload();

    } catch (error) {
      console.error("Error archiving record:", error);
    }
  };

  const handleEdit = (id) => {
    const rowToEdit = rows.find(row => row.id === id);
    setSelectedRow(id);
    setNewRow(rowToEdit);
    setOpen(true);
  };

  const handleSaveRow = async () => {
    try {
        if (selectedRow !== null) {
            // Update existing Irasas
            const updatedIrasas = {
                id: selectedRow,
                name: newRow.name,
                nr: newRow.nr,
                startdate: newRow.startdate,
                enddate: newRow.enddate,
                man: newRow.man,
                email: newRow.email,
                days: newRow.days,
                freq: newRow.freq
            };

            // Preserve the existing Prekių Adminai
            const existingPrekesAdminas = rows.find(row => row.id === selectedRow)?.prekesAdminas || [];

            // Call the API to update the Irasas (without modifying Prekių Adminai)
            const updatedData = await updateIrasas(updatedIrasas);

            // Update the row in the DataGrid while keeping the existing Prekių Adminai
            setRows(rows.map(row => (row.id === selectedRow ? { ...updatedData, prekesAdminas: existingPrekesAdminas } : row)));
        } else {
            // Create a new Irasas
            const irasas = new Irasas ({
                id_dokumento: newRow.nr,
                pavadinimas: newRow.name,
                isigaliojimo_data: new Date(newRow.startdate).toISOString(),
                pabaigos_data: new Date(newRow.enddate).toISOString(),
                dienos_pries: parseInt(newRow.days) || "0",
                dienu_daznumas: parseInt(newRow.freq) || "0",
                pastas_kreiptis: newRow.email,
                archyvuotas: false,
                naudotojai: []
            });

            const customerIds = newRow.customers.map(customer => customer.id); // Extract IDs of selected customers
            const createdIrasas = await createIrasas(irasas, customerIds); // Call the API to create the Irasas

            window.location.reload();
        }

        setOpen(false);
        setSelectedRow(null);
        setNewRow({ name: "", nr: "", startdate: "", enddate: "", man: "", email: "", days: "", freq: "", customers: [] });
    } catch (error) {
        console.error("Error saving Irasas:", error);
    }
};

  const columns = [
    { field: "name", headerName: "Sutarties Pavadinimas", flex: 2 },
    { field: "nr", headerName: "DBSIS registracijos Nr.", flex: 2 },
    { field: "startdate", headerName: "Įsigaliojimo data", flex: 2 },
    { field: "enddate", headerName: "Pabaigos data", flex: 2 },
    { field: "email", headerName: "Perspėti el. paštu  - adresas", flex: 2 },
    { field: "days", headerName: "Prieš kiek dienų iki pabaigos teikti priminimus", flex: 2 },
    { field: "freq", headerName: "Kas kiek dienų siųsti priminimą", flex: 2 },
    {
      field: "prekesAdminas",
      headerName: "Prekių Adminai",
      flex: 3,
      renderCell: (params) => (
        <div style={{ display: "flex", flexDirection: "column" }}>
          {params.row.prekesAdminas.map((admin, index) => (
            <div key={index}>
              {admin}
            </div>
          ))}
        </div>
      )
    },
    {
      field: "actions",
      headerName: "Veiksmai",
      flex: 3,
      renderCell: (params) => (
        <>
          <Button variant="contained" color="primary" size="small" /*onClick={() => handleEdit(params.row.id)}*/>Redaguoti</Button>
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
      <Typography variant="h4" gutterBottom sx={{ ml: -60 }}>Sutarčių įrašai</Typography>
      <Button variant="contained" color="success" sx={{ mb: 2, ml: -60 }} onClick={() => {
        setOpen(true);
        setSelectedRow(null);
        setNewRow({ name: "", nr: "", startdate: "", enddate: "", man: "", email: "", days: "", freq: "", customers: [] });
      }}
      >
        Pridėti naują įrašą
      </Button>
      <Button variant="contained" color="secondary" sx={{ mb: 2, ml: 2 }} onClick={() => navigate("/archived")}>
        Archyvuoti įrašai
      </Button>
      <Box sx={{ height: 400, width: "190%", ml: -60 }}>
          <DataGrid getRowId={(row) => row.id}
          rows={rows}
          columns={columns}
          pageSize={7}
          getRowHeight={() => 'auto'} // Dynamically adjust row height
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
          <TextField label="Perspėti el. paštu  - adresas" fullWidth margin="dense" type="email" value={newRow.email} onChange={(e) => setNewRow({ ...newRow, email: e.target.value })} />
          <TextField
            label="Prieš kiek dienų iki pabaigos teikti priminimus"
            fullWidth
            margin="dense"
            type="number"
            value={newRow.days}
            onChange={(e) => {
              setNewRow({ ...newRow, days: e.target.value });
            }}
          />
          <TextField
            label="Kas kiek dienų siųsti priminimą"
            fullWidth
            margin="dense"
            type="number"
            value={newRow.freq}
            onChange={(e) => {
              setNewRow({ ...newRow, freq: e.target.value });
            }}
          />
          <Autocomplete
            multiple
            options={availableCustomers}
            getOptionLabel={(option) => `${option.vardas} ${option.pavarde}, ${option.pareigos}`}
            value={newRow.customers}
            onChange={(event, newValue) => setNewRow({ ...newRow, customers: newValue })}
            renderInput={(params) => <TextField {...params} label="Prekių administratoriai" fullWidth margin="dense" />}
          />
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