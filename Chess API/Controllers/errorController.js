module.exports = (error, req, res, next) => {
  const statusCode = error.statusCode || 500
  res.status(statusCode).json({
    status: statusCode,
    message: error.message,
    stackTrace: error.stack,
    error
  });
}