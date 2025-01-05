import { Stack } from "@fluentui/react";
import { useNavigate } from "react-router-dom";
import { Genre } from "../../Enums/Genre";
import { dateAsString } from "../../utils";
import { movieContainerClassName, titleClassName } from "./movieShallow.styles";
import { MovieShallowProps } from "./movieShallow.types";

export const MovieShallow = (props: MovieShallowProps): JSX.Element => {
    const navigate = useNavigate();

    return (
        <Stack className={movieContainerClassName} onClick={() => navigate(`/reviews/${props.movie.movieId}`)}>
            <div className={titleClassName}>
                {props.movie.title}
            </div>
            <div>
                Genre: {Genre[props.movie.genre]}
            </div>
            <div>
                Release Date: {dateAsString(new Date(props.movie.apparitionDate))}
            </div>
            <div>
                Added by: {props.movie.userName}
            </div>
        </Stack>
    );
};