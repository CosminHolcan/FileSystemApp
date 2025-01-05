import { Icon, Label, Stack, TextField } from "@fluentui/react";
import React from "react";
import { IReview } from "../../Models/Review";
import { buttonClassName, errorMessageClassName, iconClassName } from "../../Pages/Movies/moviesPage.styles";
import { ReviewsService } from "../../services";
import { reviewComponentContainerClassName, textInputStyles } from "./reviewComponent.styles";
import { ReviewComponentProps } from "./reviewComponent.types";

export const ReviewComponent = (props: ReviewComponentProps): JSX.Element => {
    const [text, setText] = React.useState<string>(props.review.text);
    const [wasEdited, setWasEdited] = React.useState<boolean>(props.review.wasEdited);
    const [errorMessage, setErrorMessage] = React.useState<string>("");

    const isUserOwneOfReview: boolean = React.useMemo(() => {
        return props.review.userId === localStorage.getItem("userId");
    }, []);

    const handleSaveReview = (): void => {
        if (text === "") {
            setErrorMessage("Content of review can't be empty.");
            return;
        }

        const newReview: IReview = {
            reviewId: props.review.reviewId,
            movieId: props.review.movieId,
            movieTitle: props.review.movieTitle,
            movieYear: props.review.movieYear,
            userId: localStorage.getItem("userId") as string,
            userName: localStorage.getItem("userName") as string,
            text: text,
            creationTime: new Date(props.review.creationTime),
            wasEdited: true
        };

        ReviewsService.UpdateReview(newReview)
            .then((response) => {
                setWasEdited(true);
                props.onSavedReview(newReview);
            })
            .catch((error) => {
                setErrorMessage(error.response.data);
            });
    };

    const handleDeleteReview = (): void => {
        ReviewsService.DeleteReview(props.review.reviewId)
            .then((response) => {
                props.onDeleteReivew(props.review.reviewId);
            })
            .catch((error) => {
                setErrorMessage(error.response.data);
            });
    };

    return (
        <Stack className={reviewComponentContainerClassName}>
            {props.review.movieTitle &&
                <div style={{marginBottom: "10px"}}>{`${props.review.movieTitle} (${props.review.movieYear})`}</div>
            }
            <Stack horizontal tokens={{ childrenGap: 200 }}>
                <TextField
                    value={text}
                    multiline
                    onChange={(event, newValue) => setText(newValue ?? "")}
                    styles={textInputStyles}
                    disabled={!isUserOwneOfReview}
                />
                <Stack tokens={{ childrenGap: 10 }}>
                    {isUserOwneOfReview
                        ?
                        <button className={buttonClassName} onClick={handleSaveReview}>
                            <Icon
                                className={iconClassName}
                                iconName="Save"
                            />
                            Save
                        </button>
                        : <div style={{ height: "25px" }} />
                    }
                    {isUserOwneOfReview
                        ?
                        <button className={buttonClassName} onClick={handleDeleteReview}>
                            <Icon
                                className={iconClassName}
                                iconName="Delete"
                            />
                            Delete
                        </button>
                        : <div style={{ height: "25px" }} />
                    }
                    {wasEdited
                        ? <div>
                            Edited
                            <Icon
                                className={iconClassName}
                                iconName="Edit" />
                        </div>
                        : <div style={{ height: "25px" }} />
                    }
                    <div>
                        Added by {props.review.userName}
                    </div>
                </Stack>
            </Stack>
            {errorMessage !== "" &&
                <Label className={errorMessageClassName}>
                    {errorMessage}
                </Label>
            }
        </Stack>
    )
}