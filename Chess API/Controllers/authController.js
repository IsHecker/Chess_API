const User = require('../Models/userModel');
const asyncErrorHandler = require('../Utils/asyncErrorHandler');
const statusCode = require('../Utils/statusCodes');
const responseObj = require('../Utils/responseObject');
const CustomError = require('../Utils/CustomError');
const bcrypt = require('bcryptjs');
const jwt = require('jsonwebtoken');

const secretKey = '040930vk34cmvmvmlkvi4~*#lckvj!@=';

const generateToken = id => {
  return jwt.sign({ id }, secretKey);
}

const createAuthResponse = (user, statusCode, res) => {
  const token = generateToken(user._id);
  res.status(statusCode).json({
    status: "Success",
    token,
    data: {
      user
    }
  });
}

exports.signUp = asyncErrorHandler(async (req, res, next) => {
  req.body.password = await bcrypt.hash(req.body.password, 8);
  const user = await User.create(req.body);
  createAuthResponse(user, statusCode.created, res);
});

exports.login = asyncErrorHandler(async (req, res, next) => {
  const { email, password } = req.body;
  if (!email || !password) {
    return next(new CustomError(statusCode.badRequest, 'Provide both email and password to Login!'));
  }

  const user = await User.findOne({ email, role: req.body.role }).select('+password');

  if (!user || !await user.comparePasswords(password)) {
    return next(new CustomError(statusCode.badRequest, 'Incorrect Email or Password!'))
  }

  delete user._doc.password;

  createAuthResponse(user, statusCode.ok, res);
});

exports.authenticateToken = asyncErrorHandler(async (req, res, next) => {
  const authorization = req.headers.authorization;
  let token;
  if (authorization && authorization.startsWith('Bearer')) {
    token = authorization.split(' ')[1];
  }

  if (!token) {
    return next(new CustomError(statusCode.unauthorized, 'Access Denied'));
  }
  const payLoad = jwt.verify(token, secretKey);
  const user = await User.findById(payLoad.id);
  if (!user) {
    return next(new CustomError(statusCode.unauthorized, 'No User with this Token'));
  }
  req.user = user;
  next();
});

exports.authorize = (role) => {
  return (req, res, next) => {
    if (req.user.role != role) {
      return next(new CustomError(statusCode.forbidden, 'Unauthorized Access'))
    }
    next();
  }
}