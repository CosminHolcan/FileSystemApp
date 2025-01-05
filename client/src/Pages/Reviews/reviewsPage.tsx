import { DatePicker, Dropdown, IDropdownOption, Icon, Label, Modal, Stack, TextField } from "@fluentui/react";
import React from "react";
import { useNavigate, useParams } from "react-router-dom";
import { titleInputStyles } from "../../Components/CreateMovie/createMovie.styles";
import { CreateReview } from "../../Components/CreateReview/createReview";
import { ReviewComponent } from "../../Components/ReviewComponent/reviewComponent";
import { Genre } from "../../Enums/Genre";
import { IMovie } from "../../Models/Movie";
import { IReview } from "../../Models/Review";
import { MoviesService, ReviewsService } from "../../services";
import { buttonClassName, errorMessageClassName, iconClassName } from "../Movies/moviesPage.styles";
import { reviewPageContainerClassName } from "./reviewsPage.styles";

export const ReviewsPage = (): JSX.Element => {
    const { movieId } = useParams<{ movieId: string }>();

    const navigate = useNavigate();

    const [title, setTitle] = React.useState<string>("");
    const [genre, setGenre] = React.useState<Genre>();
    const [apparitionDate, setApparitionDate] = React.useState<Date>();
    const [isUserOwnerOfMovie, setIsUserOwnerOfMovie] = React.useState<boolean>(false);
    const [reviews, setReviews] = React.useState<IReview[]>([]);
    const [errorMessage, setErrorMessage] = React.useState<string>('');
    const [isModalOpen, setIsModalOpen] = React.useState<boolean>(false);
    const [isLoading, setIsLoading] = React.useState<boolean>(true);

    React.useEffect(() => {
        MoviesService.ReadMovie(movieId as string)
            .then((response) => {
                setIsUserOwnerOfMovie(response.data.userId === localStorage.getItem("userId"));
                setTitle(response.data.title);
                setGenre(response.data.genre);
                setApparitionDate(new Date(response.data.apparitionDate));
            })
            .catch((error) => {
                setErrorMessage(error.response.data);
            });

        ReviewsService.ReadReviewsByMovie(movieId as string)
            .then((response) => {
                const newReviews: IReview[] = response.data;
                const newReviewsSorted: IReview[] = newReviews.sort((a: IReview, b: IReview) => a.creationTime < b.creationTime ? -1 : a.creationTime === b.creationTime ? 0 : 1);
                setReviews(newReviewsSorted);
                setIsLoading(false);
            })
            .catch((error) => {
                setErrorMessage(error.response.data);
            });
    }, []);

    const genresOptions: IDropdownOption[] = [
        { key: Genre.Action, text: "Action" },
        { key: Genre.Drama, text: "Drama" },
        { key: Genre.Comedy, text: "Comedy" },
        { key: Genre.Romance, text: "Romance" },
        { key: Genre.ScienceFiction, text: "Science Fiction" },
        { key: Genre.Thriller, text: "Thriller" }
    ];

    const onChangedGenre = (event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption): void => {
        if (option) {
            setGenre(option.key as Genre);
        }
    };

    const onSelectDate = (date: Date | null | undefined): void => {
        if (date) {
            setApparitionDate(date);
        }
    };

    const handleSaveMovie = (): void => {
        let newErrorMessage: string = "";
        if (title === "") {
            newErrorMessage += "Title can't be empty.";
        }

        if (genre === null || genre === undefined) {
            newErrorMessage += " Select a genre.";
        }

        if (apparitionDate === null || apparitionDate === undefined) {
            newErrorMessage += " Select a date.";
        }

        if (newErrorMessage !== "") {
            setErrorMessage(newErrorMessage);
            return;
        }

        const newMovie: IMovie = {
            movieId: movieId as string,
            userId: localStorage.getItem("userId") as string,
            title: title,
            genre: genre as Genre,
            apparitionDate: apparitionDate as Date
        };

        MoviesService.UpdateMovie(newMovie)
            .then((function (response) {
            }))
            .catch(function (error) {
                setErrorMessage(error.response.data);
            });
    };

    const handleDeleteMovie = (): void => {
        MoviesService.DeleteMovie(movieId as string)
            .then((function (response) {
                navigate('/movies');
            }))
            .catch(function (error) {
                setErrorMessage(error.response.data);
            });
    };

    const onUpdateReview = (newReview: IReview): void => {
        const existingReviewIndex: number = reviews.findIndex((r: IReview) => r.reviewId === newReview.reviewId);
        if (existingReviewIndex === -1) {
            return;
        }

        const newReviews = [...reviews];
        newReviews[existingReviewIndex] = { ...newReview };
        setReviews(newReviews);
    };

    const onDeleteReview = (reviewId: string): void => {
        const existingReviewIndex: number = reviews.findIndex((r: IReview) => r.reviewId === reviewId);
        if (existingReviewIndex === -1) {
            return;
        }

        const newReviews: IReview[] = [...reviews];
        newReviews.splice(existingReviewIndex, 1);
        setReviews(newReviews);
    };

    const onCreateReview = (newReview: IReview): void => {
        newReview.userName = localStorage.getItem("userName") as string;
        const newReviews: IReview[] = [newReview, ...reviews];
        setReviews(newReviews);
        setIsModalOpen(false);
    };

    const logout = (): void => {
        localStorage.removeItem("userId");
        navigate("/login");
    };

    return (
        <Stack className={reviewPageContainerClassName}>
            {errorMessage !== '' &&
                <Label className={errorMessageClassName}>
                    {errorMessage}
                </Label>}
            <Modal isOpen={isModalOpen} onDismiss={() => setIsModalOpen(false)}>
                <CreateReview
                    movieId={movieId as string}
                    onSaveReview={onCreateReview}
                />
            </Modal>
            {!isLoading &&
                <div style={{ width: "1400px" }}>
                    <Stack horizontal horizontalAlign="space-between">
                        <Stack tokens={{ childrenGap: 25 }}>
                            <TextField
                                value={title}
                                onChange={(event, newValue) => setTitle(newValue ?? "")}
                                styles={titleInputStyles}
                                disabled={!isUserOwnerOfMovie}
                            />
                            <Dropdown
                                options={genresOptions}
                                defaultSelectedKey={genre}
                                onChange={onChangedGenre}
                                disabled={!isUserOwnerOfMovie}
                            />
                            <DatePicker
                                value={apparitionDate}
                                allowTextInput={false}
                                onSelectDate={onSelectDate}
                                disabled={!isUserOwnerOfMovie}
                            />
                        </Stack>
                        <Stack horizontal tokens={{ childrenGap: 20 }}>
                            {isUserOwnerOfMovie &&
                                <button className={buttonClassName} onClick={handleSaveMovie}>
                                    <Icon
                                        className={iconClassName}
                                        iconName="Save"
                                    />
                                    Save
                                </button>
                            }
                            {isUserOwnerOfMovie &&
                                <button className={buttonClassName} onClick={handleDeleteMovie}>
                                    <Icon
                                        className={iconClassName}
                                        iconName="Delete"
                                    />
                                    Delete
                                </button>
                            }
                            <button className={buttonClassName} onClick={() => setIsModalOpen(true)}>
                                <Icon
                                    className={iconClassName}
                                    iconName="Add"
                                />
                                Add Review
                            </button>
                            <button className={buttonClassName} onClick={() => navigate("/movies")}>
                                <Icon
                                    className={iconClassName}
                                    iconName="GoToDashboard"
                                />
                                Go To Movies
                            </button>
                            <button className={buttonClassName} onClick={logout}>
                                <Icon
                                    iconName="SignOut"
                                    className={iconClassName}
                                />
                                Logout
                            </button>
                        </Stack>
                    </Stack>
                    <Stack>
                        {reviews.map((review: IReview) => (
                            <ReviewComponent
                                key={review.reviewId}
                                review={review}
                                onSavedReview={onUpdateReview}
                                onDeleteReivew={onDeleteReview}
                            />
                        ))}
                    </Stack>
                </div>
            }
        </Stack>
    )
};