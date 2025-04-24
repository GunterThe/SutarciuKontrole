import axios from 'axios';
import jwtDecode from "jwt-decode";

const API_BASE_URL = 'https://localhost:5046/api';

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
            window.location.href = "/login";
        }
        return Promise.reject(error);
    }
);

function getUsernameFromToken(token) {
    try {
        const decodedToken = jwtDecode(token);
        return decodedToken.sub;
    } catch (error) {
        console.error("Invalid token:", error);
        return null;
    }
}

export const checkNaudotojas = async () => {
    try {
        const token = localStorage.getItem("token");
        const username = getUsernameFromToken(token);
        const response = await apiClient.get(`/Naudotojas/${username}`);
        return response.data;
    } catch (error) {
        if (error.response?.status === 404) {
            // To do add integration for creating a new user using different database
        }
        throw error;
    }
};

export const getIrasai = async () => {
    const response = await apiClient.get('/Irasas');
    return response.data;
};

export const getIrasasById = async (id, archived) => {
    const response = await apiClient.get(`${id}/Irasai`, {
        params: { archived }
    });
    return response.data;
}

export const createIrasas = async (irasas, ids) => {
    try {
        const token = localStorage.getItem("token");
        const username = getUsernameFromToken(token);
        const response = await apiClient.post('/Irasas', irasas);

        await apiClient.post(`/IrasasNaudotojas`, null, {
            params: {
                irasaId: response.data.Id,
                naudotojasId: username,
                adminas: false,
            },
        });

        const userPromises = ids.map((id) =>
            apiClient.post(`/IrasasNaudotojas`, null, {
                params: {
                    irasaId: response.data.Id,
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
        const token = response.data?.token;
        if (token) {
            localStorage.setItem("token", token);
            checkNaudotojas(username);
            return response.data;
        } else {
            throw "Invalid response structure: token not found";
        }
    } catch (error) {
        throw error.response?.data?.message || "Neteisingas slaptažodis arba prisijungimo vardas";
    }
};

export const logout = () => {
    localStorage.removeItem("token");
    window.location.href = "/login";
};
