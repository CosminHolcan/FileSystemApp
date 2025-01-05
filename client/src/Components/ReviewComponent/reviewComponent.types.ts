import { IReview } from "../../Models/Review";

export interface ReviewComponentProps {
    review: IReview,
    onSavedReview: (newReview: IReview) => void,
    onDeleteReivew: (reviewId: string) => void
};