import { initializeIcons } from '@fluentui/react';
import { Route, BrowserRouter as Router, Routes } from 'react-router-dom';
import PrivateRoute from './Components/PrivateRoute/privateRoute';
import { IPrivateRouteProps } from './Components/PrivateRoute/privateRoute.types';
import { HomePage } from './Pages/Home/homePage';
import { LoginPage } from './Pages/Login/loginPage';
import { RegisterPage } from './Pages/Register/registerPage';
import { UsersService } from './services';

export const App = (): JSX.Element => {
  const isUserAuthenticated = (): boolean => {
    return localStorage.getItem("jwt") != null;
  }

  const defaultProtectedRouteProps: Omit<IPrivateRouteProps, 'outlet'> = {
    authenticationPath: '/login',
  };

  initializeIcons();

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

  return (
    <Router>
      <Routes>
        <Route path='/' element={isUserAuthenticated() ? <HomePage /> : <LoginPage />} />
        <Route path='/login' element={<LoginPage />} />
        <Route path='/register' element={<RegisterPage />} />
        <Route path='/home' element={<PrivateRoute {...defaultProtectedRouteProps} outlet={<HomePage />} />} />
        {/* <Route path='/movies' element={<PrivateRoute {...defaultProtectedRouteProps} outlet={<MoviesPage />} />} />
        <Route path='/myMovies' element={<PrivateRoute {...defaultProtectedRouteProps} outlet={<MoviesPage />} />} />
        <Route path='/reviews/:movieId' element={<PrivateRoute {...defaultProtectedRouteProps} outlet={<ReviewsPage />} />} />
        <Route path='/myReviews' element={<PrivateRoute {...defaultProtectedRouteProps} outlet={<MyReviewsPage />} />} /> */}
      </Routes>
    </Router>
  );
};