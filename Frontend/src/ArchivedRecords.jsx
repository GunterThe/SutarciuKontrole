import React, { useState } from "react";
import { Container, Typography, Button, Box } from "@mui/material";
import { DataGrid } from "@mui/x-data-grid";
import { useNavigate } from "react-router-dom";

const ArchivedRecords = () => {
  const navigate = useNavigate();
  const [archivedRows, setArchivedRows] = useState(JSON.parse(localStorage.getItem("archivedRecords")) || []);

  const columns = [
    { field: "name", headerName: "Sutarties Pavadinimas", flex: 2 },
    { field: "nr", headerName: "DBSIS registracijos Nr.", flex: 2 },
    { field: "startdate", headerName: "Įsigaliojimo data", flex: 2 },
    { field: "enddate", headerName: "Pabaigos data", flex: 2 },
    { field: "man", headerName: "Atsakingas už sutarties vykdymą", flex: 2 },
    { field: "email", headerName: "Perspėti el. paštu", flex: 2 },
  ];

  return (
    <Container maxWidth="lg" sx={{ mt: 7 }}>
      <Typography variant="h4" gutterBottom>Archyvuoti įrašai</Typography>
      <Button variant="contained" color="primary" sx={{ mb: 2 }} onClick={() => navigate("/home")}>
        Grįžti
      </Button>
      <Box sx={{ height: 400, width: "100%" }}>
        <DataGrid rows={archivedRows} columns={columns} pageSize={7} 
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