import axios from "axios";
import { IMovie } from "./Models/Movie";
import { IReview } from "./Models/Review";
import { ILoginUserDTO } from "./DTO/LoginUserDTO";
import { IRegisterUserDTO } from "./DTO/RegisterUserDTO";

// const BASE_URL = "https://filesystemapp-erekcgcnhzcnbrgt.westeurope-01.azurewebsites.net";
const BASE_URL = "https://localhost:7263";

export namespace UsersService {
    const USERS_URL = `${BASE_URL}/Users`;

    export const LoginUser = (user: ILoginUserDTO) => {
        return axios.post(`${USERS_URL}/login`, user);
    };

    export const RegisterUser = (user: IRegisterUserDTO) => {
        return axios.post(`${USERS_URL}/register`, user);
    };
};

export namespace MoviesService {
    const MOVIES_URL = `${BASE_URL}/Movies`;

    export const ReadAllMovies = () => {
        return axios.get(MOVIES_URL);
    };

    export const ReadMovie = (movieId: string) => {
        return axios.get(`${MOVIES_URL}/${movieId}`);
    };

    export const ReadMoviesByUser = (userId: string) => {
        return axios.get(`${MOVIES_URL}/user/${userId}`);
    };

    export const CreateMovie = (movie: IMovie) => {
        return axios.post(MOVIES_URL, movie);
    };

    export const UpdateMovie = (movie: IMovie) => {
        return axios.put(MOVIES_URL, movie);
    };

    export const DeleteMovie = (movieId: string) => {
        return axios.delete(`${MOVIES_URL}/${movieId}`);
    };
};

export namespace ReviewsService {
    const REVIEWS_URL = `${BASE_URL}/Reviews`;

    export const ReadReviewsByMovie = (movieId: string) => {
        return axios.get(`${REVIEWS_URL}/${movieId}`);
    };

    export const ReadReviewsByUser = (userId: string) => {
        return axios.get(`${REVIEWS_URL}/user/${userId}`);
    };

    export const CreateReview = (review: IReview) => {
        return axios.post(REVIEWS_URL, review);
    };

    export const UpdateReview = (review: IReview) => {
        return axios.put(REVIEWS_URL, review);
    };

    export const DeleteReview = (reviewId: string) => {
        return axios.delete(`${REVIEWS_URL}/${reviewId}`);
    };
};
