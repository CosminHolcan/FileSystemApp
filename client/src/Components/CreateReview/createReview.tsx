import React from "react";
import { CreateReviewProps } from "./createReview.types";
import { Icon, Label, Stack, TextField } from "@fluentui/react";
import { IReview } from "../../Models/Review";
import { ReviewsService } from "../../services";
import { createReviewContainerClassName, textInputStyles } from "./createReview.styles";
import { buttonClassName, errorMessageClassName, iconClassName } from "../../Pages/Movies/moviesPage.styles";

export const CreateReview = (props: CreateReviewProps): JSX.Element => {
    const [text, setText] = React.useState<string>("");
    const [errorMessage, setErrorMessage] = React.useState<string>("");

    React.useEffect(() => {
        if (errorMessage !== "") {
            setErrorMessage("")
        }
    }, [text]);

    const handleSaveReview = (): void => {
        if (text === "") {
            setErrorMessage("Content of the review can't be empty.")
            return;
        }

        const newReview: IReview = {
            reviewId: "00000000-0000-0000-0000-000000000000",
            movieId: props.movieId,
            userId: localStorage.getItem("userId") as string,
            text: text,
            creationTime: new Date(),
            wasEdited: false
        };

        ReviewsService.CreateReview(newReview)
            .then((function (response) {
                props.onSaveReview(response.data);
            }))
            .catch(function (error) {
                setErrorMessage(error.response.data);
            });
    };

    return (
        <Stack className={createReviewContainerClassName}>
            <TextField
                value={text}
                multiline={true}
                onChange={(event, newValue) => setText(newValue ?? "")}
                styles={textInputStyles}
            />
            <button className={buttonClassName} style={{margin: "75px 0px 25px 0px"}} onClick={handleSaveReview}>
                <Icon
                    iconName="Save"
                    className={iconClassName}
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