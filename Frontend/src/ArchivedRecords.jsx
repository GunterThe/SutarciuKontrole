import React, { useState, useEffect } from "react";
import { Container, Typography, Button, Box } from "@mui/material";
import { DataGrid } from "@mui/x-data-grid";
import { useNavigate } from "react-router-dom";
import { jwtDecode } from "jwt-decode";
import { getIrasasById } from "./api"; // Import the API function to fetch archived records

const ArchivedRecords = () => {
  const token = localStorage.getItem("token");
  const id = token ? jwtDecode(token).sub : null; // Decode the user ID from the token

  const navigate = useNavigate();
  const [archivedRows, setArchivedRows] = useState([]); // State to store archived records

  useEffect(() => {
    // Fetch archived records for the user
    const fetchArchivedRecords = async () => {
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

    if (id) {
      fetchArchivedRecords();
    }
  }, [id]);

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
    }
  ];

  return (
    <Container maxWidth="lg" sx={{ mt: 7 }}>
      <Typography variant="h4" gutterBottom>Archyvuoti įrašai</Typography>
      <Button variant="contained" color="primary" sx={{ mb: 2 }} onClick={() => navigate("/home")}>
        Grįžti
      </Button>
      <Box sx={{ height: 400, width: "100%" }}>
        <DataGrid getRowId={(row) => row.id}
          rows={archivedRows}
          columns={columns}
          pageSize={7}
          getRowHeight={() => 'auto'} 
          localeText={{
            noRowsLabel: "Nėra duomenų",
            toolbarDensity: "Eilutės per puslapį",
            MuiTablePagination: {
              labelRowsPerPage: "Eilučių per puslapį",
            }
          }}
        />
      </Box>
    </Container>
  );
};

export default ArchivedRecords;