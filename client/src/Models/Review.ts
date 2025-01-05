export interface IReview {
    reviewId: string;
    movieId: string;
    movieTitle?: string;
    movieYear?: number;
    userId: string;
    userName?: string;
    text: string;
    wasEdited: boolean;
    creationTime: Date;
};