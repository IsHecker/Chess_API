const Puzzle = require('../Models/puzzleModel');
const User = require('../Models/userModel');
const asyncErrorHandler = require('../Utils/asyncErrorHandler');
const statusCode = require('../Utils/statusCodes');
const responseObj = require('../Utils/responseObject');
const CustomError = require('../Utils/CustomError');

exports.getUser = asyncErrorHandler(async (req, res) => {
  const filter = JSON.stringify(req.body) === '{}' ? req.user._id : req.body;
  const user = await User.findOne(filter);
  res.status(statusCode.ok).json(responseObj('Success', user));
});


exports.addPuzzle = asyncErrorHandler(async (req, res) => {
  const user = await User.findByIdAndUpdate(req.user._id, {
    $addToSet: { puzzles: req.body._id }
  });

  if (!user) {
    return next(new CustomError(statusCode.notFound), 'User is Not Found');
  }

  res.status(statusCode.created).json(responseObj('Success', user));
});
