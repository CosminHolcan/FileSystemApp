import { Label, Stack, StackItem, TextField } from "@fluentui/react";
import React from "react";
import { useNavigate } from "react-router-dom";
import { IRegisterUserDTO } from "../../DTO/RegisterUserDTO";
import { UsersService } from "../../services";
import { authButtonLargeStyle, authButtonLargeWithMarginStyle, authErrorMessageSmallStyle, authLabelSmallStyle, authPageBackgroundStyle, fieldContainerStyle, registerFormContainerStyle, repeatPasswordFieldContainerStyle } from "../../styles";

export const RegisterPage = (): JSX.Element => {
    const navigate = useNavigate();
    const [email, setEmail] = React.useState<string>('');
    const [password, setPassword] = React.useState<string>('');
    const [repeatPassword, setRepeatPassword] = React.useState<string>('');
    const [firstName, setFirstName] = React.useState<string>('');
    const [lastName, setLastName] = React.useState<string>('');
    const [errorMessage, setErrorMessage] = React.useState<string>('');

    React.useEffect(() => {
        setErrorMessage('');
    }, [email, password, repeatPassword, firstName, lastName]);

    const handleSubmit = async (e: any) => {
        var newErrorMessage: string = '';
        if (email.trim() === "" || password.trim() === "" || firstName.trim() === "" || lastName.trim() === "") {
            newErrorMessage += "All fields are required, none of them can be empty."
        }

        if (password !== repeatPassword) {
            newErrorMessage += "The password and repeat password fields don't match.";
        }

        if (newErrorMessage !== '') {
            setErrorMessage(newErrorMessage);
            return;
        }

        const registerDTO: IRegisterUserDTO = {
            email: email,
            password: password,
            firstName: firstName,
            lastName: lastName
        };

        UsersService.RegisterUser(registerDTO)
            .then(function (response) {
                localStorage.setItem("jwt", response.data.Jwt);
                localStorage.setItem("userName", response.data.FirstName + " " + response.data.LastName);
                navigate("/home");
            })
            .catch(function (error) {
                setErrorMessage(error.response.data)
            });
    }

    const redirectLoginPage = () => {
        navigate("/login");
    }

    return (
        <Stack style={authPageBackgroundStyle} horizontalAlign="center" verticalAlign="center">
            <Stack style={registerFormContainerStyle}>
                <StackItem style={fieldContainerStyle}>
                    <Label style={authLabelSmallStyle}>
                        Email
                    </Label>
                    <TextField
                        rows={1}
                        value={email}
                        onChange={(event: any) => setEmail(event.target.value)}
                    />
                </StackItem>
                <StackItem style={fieldContainerStyle}>
                    <Label style={authLabelSmallStyle}>
                        First Name
                    </Label>
                    <TextField
                        rows={1}
                        value={firstName}
                        onChange={(event: any) => setFirstName(event.target.value)}
                    />
                </StackItem>
                <StackItem style={fieldContainerStyle}>
                    <Label style={authLabelSmallStyle}>
                        Last Name
                    </Label>
                    <TextField
                        rows={1}
                        value={lastName}
                        onChange={(event: any) => setLastName(event.target.value)}
                    />
                </StackItem>
                <StackItem style={fieldContainerStyle}>
                    <Label style={authLabelSmallStyle}>
                        Password
                    </Label>
                    <TextField
                        type="password"
                        rows={1}
                        value={password}
                        onChange={(event: any) => setPassword(event.target.value)}
                    />
                </StackItem>
                <StackItem style={repeatPasswordFieldContainerStyle}>
                    <Label style={authLabelSmallStyle}>
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
                    <button style={authButtonLargeWithMarginStyle} onClick={handleSubmit}>Register</button>
                    <button style={authButtonLargeStyle} onClick={redirectLoginPage}>Already having an account ?</button>
                </Stack>
                <Label style={authErrorMessageSmallStyle}>
                    {errorMessage}
                </Label>
            </Stack>
        </Stack>
    )
}