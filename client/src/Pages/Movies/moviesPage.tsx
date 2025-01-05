import { Icon, Label, Modal, Stack } from "@fluentui/react";
import React from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { CreateMovie } from "../../Components/CreateMovie/createMovie";
import { MovieShallow } from "../../Components/MovieShallow/movieShallow";
import { IMovie } from "../../Models/Movie";
import { MoviesService } from "../../services";
import { buttonClassName, buttonContainerClassName, errorMessageClassName, iconClassName } from "./moviesPage.styles";

export const MoviesPage = (): JSX.Element => {
    const navigate = useNavigate();
    const location = useLocation();

    const [movies, setMovies] = React.useState<IMovie[]>([]);
    const [errorMessage, setErrorMessage] = React.useState<String>('');
    const [isModalOpen, setIsModalOpen] = React.useState<boolean>(false);

    const isAllMoviesPage: boolean = React.useMemo(() => {
        return location.pathname === "/movies" || location.pathname === "/";
    }, []);

    React.useEffect(() => {
        if (isAllMoviesPage) {
            MoviesService.ReadAllMovies()
                .then((response) => {
                    setMovies(response.data);
                })
                .catch((error) => {
                    setErrorMessage(error.response.data);
                })
        }
        else {
            MoviesService.ReadMoviesByUser(localStorage.getItem("userId") as string)
                .then((response) => {
                    setMovies(response.data);
                })
                .catch((error) => {
                    setErrorMessage(error.response.data);
                })
        }
    }, []);

    const onCreatedMovie = (newMovie: IMovie) => {
        setMovies([...movies, newMovie]);
        setIsModalOpen(false);
    };

    const logout = (): void => {
        localStorage.removeItem("userId");
        navigate("/login");
    };

    return (
        <Stack>
            <Stack>
                <Stack horizontal horizontalAlign="end" className={buttonContainerClassName} tokens={{ childrenGap: 20 }}>
                    <button className={buttonClassName} onClick={() => setIsModalOpen(true)}>
                        <Icon
                            iconName="Add"
                            className={iconClassName}
                        />
                        Add Movie
                    </button>
                    {isAllMoviesPage &&
                        <button className={buttonClassName} onClick={() => { navigate("/myMovies"); window.location.reload(); }}>
                            <Icon
                                iconName="GoToDashboard"
                                className={iconClassName}
                            />
                            My movies
                        </button>
                    }
                    {isAllMoviesPage &&
                        <button className={buttonClassName} onClick={() => { navigate("/myReviews"); window.location.reload(); }}>
                            <Icon
                                iconName="ReviewSolid"
                                className={iconClassName}
                            />
                            My reviews
                        </button>
                    }
                    {!isAllMoviesPage &&
                        <button className={buttonClassName} onClick={() => { navigate("/movies"); window.location.reload(); }}>
                            <Icon
                                iconName="GoToDashboard"
                                className={iconClassName}
                            />
                            All movies
                        </button>
                    }
                    <button className={buttonClassName} onClick={logout}>
                        <Icon
                            iconName="SignOut"
                            className={iconClassName}
                        />
                        Logout
                    </button>
                </Stack>
                {movies.map((movie: IMovie) => (
                    <MovieShallow key={movie.movieId} movie={movie} />
                ))}
                <Modal isOpen={isModalOpen} onDismiss={() => setIsModalOpen(false)}>
                    <CreateMovie
                        onSavedMovie={onCreatedMovie}
                    />
                </Modal>
            </Stack>
            {
                errorMessage !== '' &&
                <Label className={errorMessageClassName}>
                    {errorMessage}
                </Label>
            }
        </Stack >
    )
};