const Puzzle = require('../Models/puzzleModel');
const User = require('../Models/userModel');
const asyncErrorHandler = require('../Utils/asyncErrorHandler');
const statusCode = require('../Utils/statusCodes');
const responseObj = require('../Utils/responseObject');
const CustomError = require('../Utils/CustomError');

exports.getPuzzles = asyncErrorHandler(async (req, res) => {
  const puzzles = await Puzzle.find(req.params).sort(req.query.sort);
  res.status(statusCode.ok).json({
    status: "Success",
    length: puzzles.length,
    data: {
      puzzles
    }
  });
});

exports.getCreatorPuzzles = asyncErrorHandler(async (req, res, next) => {

  const puzzles = await Puzzle.find({ _id: { $in: req.user.puzzles } }).sort({createdAt: -1});

  if (!puzzles) {
    return next(new CustomError(statusCode.notFound, 'No Puzzle with that ID!'));
  }

  res.status(statusCode.ok).json({
    status: "Success",
    length: puzzles.length,
    data: {
      puzzles
    }
  });
});

exports.createPuzzle = asyncErrorHandler(async (req, res) => {
  const puzzle = await Puzzle.create(req.body);

  await User.findByIdAndUpdate(req.user._id, {
    $push: { puzzles: puzzle._id }
  });

  res.status(statusCode.created).json(responseObj('Success', puzzle));
});


exports.editPuzzle = asyncErrorHandler(async (req, res, next) => {

  if (req.user.role == 'Creator')
    req.body.createdAt = Puzzle.getFullDate();

  let puzzle = await Puzzle.findByIdAndUpdate(req.body._id, req.body, { runValidators: true, new: true });

  if (!puzzle) {
    return next(new CustomError(statusCode.notFound, 'No Puzzle Found'));
  }

  res.status(statusCode.ok).json(responseObj('Success', puzzle));
});

exports.deletePuzzle = asyncErrorHandler(async (req, res, next) => {
  const puzzle = await Puzzle.findByIdAndDelete(req.body._id);
  
  if (!puzzle) {
    return next(new CustomError(statusCode.notFound, 'No Puzzle Found with that ID'));
  }

  await User.findByIdAndUpdate(req.user._id, {
    $pull: { puzzles: puzzle._id }
  });

  res.status(statusCode.noContext).json(responseObj('Success', {}));
});
