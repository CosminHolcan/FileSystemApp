import { initializeIcons } from '@fluentui/react';
import React from 'react';
import { Route, BrowserRouter as Router, Routes } from 'react-router-dom';
import PrivateRoute from './Components/PrivateRoute/privateRoute';
import { IPrivateRouteProps } from './Components/PrivateRoute/privateRoute.types';
import { HomePage } from './Pages/Home/homePage';
import { LoginPage } from './Pages/Login/loginPage';
import { RegisterPage } from './Pages/Register/registerPage';
import { VersioningPage } from './Pages/Versioning/versioningPage';
import { UsersService } from './services';

export const App = (): JSX.Element => {
  const isUserAuthenticated = (): boolean => {
    return localStorage.getItem("jwt") != null;
  }

  const defaultProtectedRouteProps: Omit<IPrivateRouteProps, 'outlet'> = {
    authenticationPath: '/login',
  };

  initializeIcons();

  React.useEffect(() => {
    var token = localStorage.getItem("jwt");
    if (token != null) {
      UsersService.RefreshToken({ jwt: token })
        .then(async (response) => {
          localStorage.setItem("jwt", response.data.jwt);
        })
        .catch(async (error) => {
          localStorage.removeItem("jwt");
          localStorage.removeItem("userName");
        })
    }
  }, []);

  return (
    <Router>
      <Routes>
        <Route path='/' element={isUserAuthenticated() ? <HomePage /> : <LoginPage />} />
        <Route path='/login' element={<LoginPage />} />
        <Route path='/register' element={<RegisterPage />} />
        <Route path='/home' element={<PrivateRoute {...defaultProtectedRouteProps} outlet={<HomePage />} />} />
        <Route path='/versioning/:fileId' element={<PrivateRoute {...defaultProtectedRouteProps} outlet={<VersioningPage />} />} />
      </Routes>
    </Router>
  );
};