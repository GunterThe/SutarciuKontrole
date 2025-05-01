import React, { useState } from "react";
import { TextField, Button, Container, Typography, Paper, Box } from "@mui/material";
import { useNavigate } from "react-router-dom";
import { login } from "./api";

const Login = () => {
  const [id, setId] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      const response = await login(id, password);
      navigate("/home", { state: { username: response.username } });
    } catch (err) {
      setError(err.message || "An error occurred");
    }
  };

  return (
    <Container component="main" maxWidth="xs">
      <Paper
        elevation={6}
        sx={{ padding: 4, display: "flex", flexDirection: "column", alignItems: "center", marginTop: 8, borderRadius: 2 }}
      >
        <Typography variant="h5" sx={{ marginBottom: 2, fontWeight: "bold" }}>Prisijungimas</Typography>
        {error && (
          <Typography color="error" variant="body2">
            {error}
          </Typography>
        )}
        <Box component="form" onSubmit={handleSubmit} sx={{ width: "100%", mt: 1 }}>
          <TextField
            fullWidth
            label="Id"
            variant="outlined"
            margin="normal"
            value={id}
            onChange={(e) => setId(e.target.value)}
            required
          />
          <TextField
            fullWidth
            label="Slaptažodis"
            type="password"
            variant="outlined"
            margin="normal"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
          <Button
            type="submit"
            fullWidth
            variant="contained"
            sx={{ mt: 2, bgcolor: "primary.main", padding: "10px" }}
          >
            Prisijungti
          </Button>
        </Box>
      </Paper>
    </Container>
  );
};

export default Login;