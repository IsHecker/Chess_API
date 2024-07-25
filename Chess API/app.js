const express = require("express");
const puzzleRouter = require('./Routes/puzzleRoutes');
const authRouter = require('./Routes/authRoutes');
const userRouter = require('./Routes/userRoutes');
const globalError = require('./Controllers/errorController');

const app = express();
app.use(express.json());


app.use('/api/auth', authRouter);
app.use('/api/users', userRouter);
app.use('/api/puzzles', puzzleRouter);
app.use(globalError);

module.exports = app;