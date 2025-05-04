import axios from 'axios';
import { jwtDecode } from "jwt-decode";

const API_BASE_URL = 'http://localhost:5046/api';

const apiClient = axios.create({
    baseURL: API_BASE_URL,
});

apiClient.interceptors.request.use((config) => {
    const token = localStorage.getItem("token");
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
}, (error) => {
    return Promise.reject(error);
});

apiClient.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response?.status === 401) {
            localStorage.removeItem("token");
            window.location.href = "/";
        }
        return Promise.reject(error);
    }
);

export const archiveIrasas = async (id) => {
    const response = await apiClient.post(`Irasas/${id}/Archive`);
    
    if (!response.ok) {
      throw new Error('Failed to archive record');
    }
    
    return await response.json();
  };

function getUsernameFromToken(token) {
    try {
        const decodedToken = jwtDecode(token);
        return decodedToken.sub;
    } catch (error) {
        console.error("Invalid token:", error);
        return null;
    }
}

export const getIrasai = async () => {
    const response = await apiClient.get('/Irasas');
    return response.data;
};

export const getIrasasById = async (id, Archyvuotas) => {
    const response = await apiClient.get(`/Naudotojas/${id}/Irasai`, {
        params: { Archyvuotas }
    });
    return response.data;
}


export const createIrasas = async (irasas, ids) => {
    try {
        const token = localStorage.getItem("token");
        const username = getUsernameFromToken(token);
        const response = await apiClient.post('/Irasas', irasas, {
            headers: {
                'Content-Type': 'application/json'
            }
        });

        await apiClient.post(`/IrasasNaudotojas`, null, {
            params: {
                irasaId: response.data.id,
                naudotojasId: username,
                adminas: false,
            },
        });

        const userPromises = ids.map((id) =>
            apiClient.post(`/IrasasNaudotojas`, null, {
                params: {
                    irasaId: response.data.id,
                    naudotojasId: id,
                    adminas: true,
                },
            })
        );
        await Promise.all(userPromises);

        return response.data;
    } catch (error) {
        console.error("Error creating Irasas:", error);
        throw error;
    }
};

export const updateIrasas = async (irasas) => {
    try {
        const response = await apiClient.put(`/Irasas/${irasas.Id}`, irasas);
        return response.data;
    } catch (error) {
        console.error("Error updating Irasas:", error);
        throw error;
    }
}

export const login = async (username, password) => {
    try {
        const response = await apiClient.post('/Auth/login', { username, password });
        console.log(response);
        const token = response.data?.token;
        if(!token) {
            throw new Error("Invalid response from server");
        }
        localStorage.setItem("token", token);
        return response.data;
    } catch (error) {
        throw error.response?.data?.message || "Neteisingas slaptažodis arba prisijungimo vardas";
    }
};

export const logout = () => {
    localStorage.removeItem("token");
    window.location.href = "/";
};

export const getIrasasNaudotojai = async (id) => {
    try {
        const response = await apiClient.get(`Irasas/${id}/Naudotojai`);
        return response.data;
    } catch (error) {
        console.error(`Error fetching Naudotojai for Irasas with ID ${id}:`, error);
        throw error;
    }
};

export const getAllNaudotojai = async () => {
    try {
        const response = await apiClient.get('/Naudotojas'); // Calls the GET: api/Naudotojas endpoint
        return response.data; // Return the list of Naudotojai
    } catch (error) {
        console.error("Error fetching all Naudotojai:", error);
        throw error;
    }
};
