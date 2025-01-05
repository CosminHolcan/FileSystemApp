import { Icon, Label, Stack } from "@fluentui/react";
import React from "react";
import { useNavigate } from "react-router-dom";
import { ReviewComponent } from "../../Components/ReviewComponent/reviewComponent";
import { IReview } from "../../Models/Review";
import { ReviewsService } from "../../services";
import { buttonClassName, errorMessageClassName, iconClassName } from "../Movies/moviesPage.styles";
import { reviewPageContainerClassName } from "./myReviewsPage.styles";

export const MyReviewsPage = (): JSX.Element => {
    const navigate = useNavigate();

    const [reviews, setReviews] = React.useState<IReview[]>([]);
    const [errorMessage, setErrorMessage] = React.useState<string>('');
    const [isLoading, setIsLoading] = React.useState<boolean>(true);

    React.useEffect(() => {
        ReviewsService.ReadReviewsByUser(localStorage.getItem("userId") as string)
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
            {!isLoading &&
                <div style={{ width: "1400px" }}>
                    <Stack horizontal horizontalAlign="end" tokens={{ childrenGap: 20 }}>
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