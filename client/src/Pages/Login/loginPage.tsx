import { Label, Stack, StackItem, TextField } from "@fluentui/react";
import React from "react";
import { useNavigate } from "react-router-dom";
import { ILoginUserDTO } from "../../DTO/LoginUserDTO";
import { UsersService } from "../../services";
import { ButtonLoginStyle, ButtonRegisterStyle, EmailContainerStyle, ErrorMessageStyle, LabelStyle, LoginContainerStyle, LoginFormContainerStyle, PasswordContainerStyle } from "./loginPage.styles";

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
        <Stack style={LoginContainerStyle} horizontalAlign="center" verticalAlign="center">
            <Stack style={LoginFormContainerStyle}>
                <StackItem style={EmailContainerStyle}>
                    <Label style={LabelStyle}>
                        Email
                    </Label>
                    <TextField
                        rows={1}
                        value={email}
                        onChange={(event: any) => { setEmail(event.target.value); }}
                    />
                </StackItem>
                <StackItem style={PasswordContainerStyle}>
                    <Label style={LabelStyle}>
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
                    <button style={ButtonLoginStyle} onClick={handleSubmit}>Log in</button>
                    <button style={ButtonRegisterStyle} onClick={redirectCreateNewAccount}>Create a new account</button>
                </Stack>
                <Label style={ErrorMessageStyle}>
                    {errorMessage}
                </Label>
            </Stack>
        </Stack>
    )
}