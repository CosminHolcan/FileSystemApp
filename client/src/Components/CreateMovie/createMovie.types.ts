import { IMovie } from "../../Models/Movie";

export interface CreateMovieProps {
    onSavedMovie: (newMovie: IMovie) => void
};
