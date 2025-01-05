import { Genre } from "../Enums/Genre";

export interface IMovie {
    movieId: string;
    userId: string;
    userName?: string;
    title: string;
    genre: Genre;
    apparitionDate: Date;
};