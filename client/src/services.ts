import axios from "axios";
import { IBaseDTO } from "./DTO/BaseDTO";
import { ILoginUserDTO } from "./DTO/LoginUserDTO";
import { IRegisterUserDTO } from "./DTO/RegisterUserDTO";

const BASE_URL = process.env.REACT_APP_API_URL;

export namespace UsersService {
    const USERS_URL = `${BASE_URL}/Users`;

    export const LoginUser = (user: ILoginUserDTO) => {
        return axios.post(`${USERS_URL}/login`, user);
    };

    export const RegisterUser = (user: IRegisterUserDTO) => {
        return axios.post(`${USERS_URL}/register`, user);
    };

    export const RefreshToken = (dto: IBaseDTO) => {
        return axios.post(`${USERS_URL}/refreshToken`, dto);
    }
};

export namespace AppFilesService {
    const APP_FILES_URL = `${BASE_URL}/AppFiles`;

    export const Addfile = (dto: FormData) => {
        return axios.post(`${APP_FILES_URL}/add`, dto, {
            headers: {
                'Content-Type': 'multipart/form-data'
            }
        });
    };

    export const ReadFilesByUser = (dto: IBaseDTO) => {
        return axios.post(`${APP_FILES_URL}/filesByUser`, dto);
    };
};
