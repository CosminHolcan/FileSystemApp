import { Label, Stack, StackItem, TextField } from "@fluentui/react";
import React from "react";
import { useNavigate } from "react-router-dom";
import { ILoginUserDTO } from "../../DTO/LoginUserDTO";
import {
    authPageBackgroundStyle,
    loginFormContainerStyle,
    authLabelStyle,
    authButtonStyle,
    authButtonWithMarginStyle,
    authErrorMessageStyle
} from "../../styles";
import { UsersService } from "../../services";

export const LoginPage = (): JSX.Element => {
    const navigate = useNavigate();
    const [email, setEmail] = React.useState<string>('');
    const [password, setPassword] = React.useState<string>('');
    const [errorMessage, setErrorMessage] = React.useState<string>('');

    React.useEffect(() => {
        setErrorMessage('');
    }, [email, password]);

    const handleSubmit = async (e: any) => {
        const loginDTO: ILoginUserDTO = {
            email: email,
            password: password
        };

        UsersService.LoginUser(loginDTO)
            .then(function (response) {
                localStorage.setItem("jwt", response.data.jwt);
                localStorage.setItem("userName", response.data.firstName + " " + response.data.lastName);
                navigate("/home");
            })
            .catch(function (error) {
                setErrorMessage(error.response.data)
            });
    }

    const redirectCreateNewAccount = () => {
        navigate("/register");
    }

    return (
        <Stack style={authPageBackgroundStyle} horizontalAlign="center" verticalAlign="center">
            <Stack style={loginFormContainerStyle}>
                <StackItem style={{ height: "5vh", width: "20vw", marginRight: "5vw", marginLeft: "5vw", marginTop: "10vh" }}>
                    <Label style={authLabelStyle}>
                        Email
                    </Label>
                    <TextField
                        rows={1}
                        value={email}
                        onChange={(event: any) => { setEmail(event.target.value); }}
                    />
                </StackItem>
                <StackItem style={{ height: "5vh", width: "20vw", marginRight: "5vw", marginLeft: "5vw", marginTop: "8vh", marginBottom: "12vh" }}>
                    <Label style={authLabelStyle}>
                        Password
                    </Label>
                    <TextField
                        type="password"
                        canRevealPassword={true}
                        rows={1}
                        value={password}
                        onChange={(event: any) => { setPassword(event.target.value); }}
                    />
                </StackItem>
                <Stack horizontalAlign="center" horizontal>
                    <button style={authButtonStyle} onClick={handleSubmit}>Log in</button>
                    <button style={authButtonWithMarginStyle} onClick={redirectCreateNewAccount}>Create a new account</button>
                </Stack>
                <Label style={authErrorMessageStyle}>
                    {errorMessage}
                </Label>
            </Stack>
        </Stack>
    )
}