import { initializeIcons } from '@fluentui/react';
import { Route, BrowserRouter as Router, Routes } from 'react-router-dom';
import PrivateRoute from './Components/PrivateRoute/privateRoute';
import { IPrivateRouteProps } from './Components/PrivateRoute/privateRoute.types';
import { LoginPage } from './Pages/Login/loginPage';
import { MoviesPage } from './Pages/Movies/moviesPage';
import { RegisterPage } from './Pages/Register/registerPage';
import { ReviewsPage } from './Pages/Reviews/reviewsPage';
import { MyReviewsPage } from './Pages/MyReviews/myReviewsPage';

export const App = (): JSX.Element => {
  const isUserAuthenticated = (): boolean => {
    return localStorage.getItem("userId") != null;
  }

  const defaultProtectedRouteProps: Omit<IPrivateRouteProps, 'outlet'> = {
    authenticationPath: '/login',
  };

  initializeIcons();

  return (
    <Router>
      <Routes>
        <Route path='/' element={isUserAuthenticated() ? <MoviesPage /> : <LoginPage />} />
        <Route path='/login' element={<LoginPage />} />
        <Route path='/register' element={<RegisterPage />} />
        <Route path='/movies' element={<PrivateRoute {...defaultProtectedRouteProps} outlet={<MoviesPage />} />} />
        <Route path='/myMovies' element={<PrivateRoute {...defaultProtectedRouteProps} outlet={<MoviesPage />} />} />
        <Route path='/reviews/:movieId' element={<PrivateRoute {...defaultProtectedRouteProps} outlet={<ReviewsPage />} />} />
        <Route path='/myReviews' element={<PrivateRoute {...defaultProtectedRouteProps} outlet={<MyReviewsPage />} />} />
      </Routes>
    </Router>
  );
};