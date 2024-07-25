const express = require('express');
const puzzleController = require('../Controllers/puzzleController');
const authController = require('../Controllers/authController');

const router = express.Router();

router.all('*', authController.authenticateToken);
router.route('/')
  .get(puzzleController.getPuzzles)
  .post(puzzleController.createPuzzle)
  .patch(puzzleController.editPuzzle)
  .delete(puzzleController.deletePuzzle);

router.get('/creator', puzzleController.getCreatorPuzzles);
router.get('/:name', puzzleController.getPuzzles);

module.exports = router;