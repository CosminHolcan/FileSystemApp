import { Label, Stack, StackItem, TextField } from "@fluentui/react";
import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { IUser } from "../../Models/User";
import { UsersService } from "../../services";
import { ButtonLoginStyle, ButtonRegisterStyle, ErrorMessageStyle, LabelStyle, MiddleFieldContainerStyle, RegisterContainerStyle, RegisterFormContainerStyle, RepeatPasswordContainerStyle } from "./registerPage.styles";

export const RegisterPage = (): JSX.Element => {
    const navigate = useNavigate();
    const [userName, setUserName] = useState<string>('');
    const [password, setPassword] = useState<string>('');
    const [repeatPassword, setRepeatPassword] = useState<string>('');
    const [errorMessage, setErrorMessage] = useState<string>('');

    useEffect(() => {
        setErrorMessage('');
    }, [userName, password, repeatPassword]);

    const handleSubmit = async (e: any) => {
        var newErrorMessage: string = '';
        if (userName.trim() === "" || password.trim() === "") {
            newErrorMessage += "All fields are required, none of them can be empty."
        }

        if (password !== repeatPassword) {
            newErrorMessage += "The password and repeat password fields don't match.";
        }

        if (newErrorMessage !== '') {
            setErrorMessage(newErrorMessage);
            return;
        }

        const registerDTO: IUser = {
            userName: userName,
            password: password
        };

        UsersService.RegisterUser(registerDTO)
            .then(function (response) {
                localStorage.setItem('userId', response.data.userId);
                navigate("/movies");
            })
            .catch(function (error) {
                setErrorMessage(error.response.data)
            });
    }

    const handleChangedEmail = (newValue: string): void => {
        if (errorMessage !== '')
            setErrorMessage('');
        setUserName(newValue);
    }

    const redirectLoginPage = () => {
        navigate("/login");
    }

    return (
        <Stack style={RegisterContainerStyle} horizontalAlign="center" verticalAlign="center">
            <Stack style={RegisterFormContainerStyle}>
                <StackItem style={MiddleFieldContainerStyle}>
                    <Label style={LabelStyle}>
                        Username
                    </Label>
                    <TextField
                        rows={1}
                        value={userName}
                        onChange={(event: any) => handleChangedEmail(event.target.value)}
                    />
                </StackItem>
                <StackItem style={MiddleFieldContainerStyle}>
                    <Label style={LabelStyle}>
                        Password
                    </Label>
                    <TextField
                        type="password"
                        rows={1}
                        value={password}
                        onChange={(event: any) => setPassword(event.target.value)}
                    />
                </StackItem>
                <StackItem style={RepeatPasswordContainerStyle}>
                    <Label style={LabelStyle}>
                        Repeat Password
                    </Label>
                    <TextField
                        type="password"
                        rows={1}
                        value={repeatPassword}
                        onChange={(event: any) => setRepeatPassword(event.target.value)}
                    />
                </StackItem>
                <Stack horizontalAlign="center" horizontal>
                    <button style={ButtonRegisterStyle} onClick={handleSubmit}>Register</button>
                    <button style={ButtonLoginStyle} onClick={redirectLoginPage}>Already having an account ?</button>
                </Stack>
                <Label style={ErrorMessageStyle}>
                    {errorMessage}
                </Label>
            </Stack>
        </Stack>
    )
}