import { Navigate } from "react-router-dom";
import { IPrivateRouteProps } from "./privateRoute.types";

export default function PrivateRoute({ authenticationPath, outlet }: IPrivateRouteProps) {
    const isAuthenticated: boolean = localStorage.getItem("userId") != null;
    
    if (isAuthenticated) {
        return outlet;
    } else {
        return <Navigate to={{ pathname: authenticationPath }} />;
    }
};