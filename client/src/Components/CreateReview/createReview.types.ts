import { IReview } from "../../Models/Review";

export interface CreateReviewProps {
    movieId: string,
    onSaveReview: (newReview: IReview) => void
};