import { DatePicker, Dropdown, IDropdownOption, Icon, Label, Stack, TextField } from "@fluentui/react";
import React from "react";
import { Genre } from "../../Enums/Genre";
import { IMovie } from "../../Models/Movie";
import { buttonClassName, errorMessageClassName, iconClassName } from "../../Pages/Movies/moviesPage.styles";
import { MoviesService } from "../../services";
import { modalContainerClassName, titleInputStyles } from "./createMovie.styles";
import { CreateMovieProps } from "./createMovie.types";

export const CreateMovie = (props: CreateMovieProps): JSX.Element => {
    const [title, setTitle] = React.useState<string>("");
    const [genre, setGenre] = React.useState<Genre>();
    const [apparitionDate, setApparitionDate] = React.useState<Date>();
    const [errorMessage, setErrorMessage] = React.useState<String>("");

    React.useEffect(() => {
        if (errorMessage !== "") {
            setErrorMessage("");
        }
    }, [title, genre, apparitionDate]);

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
            movieId: "00000000-0000-0000-0000-000000000000",
            userId: localStorage.getItem("userId") as string,
            title: title,
            genre: genre as Genre,
            apparitionDate: apparitionDate as Date,
        };

        MoviesService.CreateMovie(newMovie)
            .then((function (response) {
                props.onSavedMovie(response.data);
            }))
            .catch(function (error) {
                setErrorMessage(error.response.data);
            });
    };

    return (
        <Stack className={modalContainerClassName} verticalAlign="space-between">
            <TextField
                value={title}
                onChange={(event, newValue) => setTitle(newValue ?? "")}
                styles={titleInputStyles}
                placeholder="Title"
            />
            <Dropdown
                options={genresOptions}
                defaultSelectedKey={genre}
                onChange={onChangedGenre}
                placeholder="Genre"
            />
            <DatePicker
                value={apparitionDate}
                allowTextInput={false}
                onSelectDate={onSelectDate}
                placeholder="Apparation Date"
            />
            <button className={buttonClassName} onClick={handleSaveMovie}>
                <Icon
                    className={iconClassName}
                    iconName="Save"
                />
                Save
            </button>
            {errorMessage !== "" &&
                <Label className={errorMessageClassName}>
                    {errorMessage}
                </Label>
            }
        </Stack>
    )
}