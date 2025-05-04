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
        const archivedRecords = await getIrasasById(id, true); // Fetch archived records (archived = true)
        setArchivedRows(archivedRecords); // Set the fetched records to the state
      } catch (error) {
        console.error("Error fetching archived records:", error);
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
    { field: "man", headerName: "Atsakingas už sutarties vykdymą", flex: 2 },
    { field: "email", headerName: "Perspėti el. paštu", flex: 2 },
    {
      field: "customers",
      headerName: "Prekių administratoriai",
      flex: 3,
      renderCell: (params) => (
        <div style={{ display: "flex", flexDirection: "column" }}>
          {params.row.customers.map((customer, index) => (
            <div key={index}>
              {customer.name} {customer.lastName}, {customer.birthdate}, {customer.occupation}
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
        <DataGrid
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