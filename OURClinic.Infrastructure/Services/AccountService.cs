using Microsoft.EntityFrameworkCore;
using OurCart.DataModel;
using OurCart.Infrastructure.Services;
using OURCart.Core.IServices;
using OURCart.Core.Util;
using OURCart.DataModel.DTO;
using OURCart.DataModel.DTO.LocalModels;
using OURCart.Infrastructure.Util;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OURCart.Infrastructure.Services
{
    public class AccountService : BaseRepository<DeliveryClient>, IAccountService
    {
        public AccountService(OurCartDBContext dbContext) : base(dbContext)
        {
        }

        public async Task<OperationResponse<bool>> changePassword(ChangePasswordModel changePasswordModel)
        {
            OperationResponse<bool> response = new OperationResponse<bool>();
            try
            {
                var user = await _dbContext.DeliveryClient.Where(c => c.DelClientId == changePasswordModel.userID && c.Password == HashingUtility.hashPassword(changePasswordModel.oldPassword)).FirstOrDefaultAsync();
                if (user != null)
                {
                    user.Password = HashingUtility.hashPassword(changePasswordModel.newPassword);

                }
                var rowsAffectred = _dbContext.SaveChanges();
                if (rowsAffectred > 0)
                    response.Data = true;
                else
                {
                    response.HasErrors = true;
                    response.Message = "Error in Creating User";
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                response.HasErrors = true;
                response.Message = msg;
            }
            return response;
        }

        public async Task<OperationResponse<DeliveryClient>> createAccount(DeliveryClient registerModel)
        {
            OperationResponse<DeliveryClient> response = new OperationResponse<DeliveryClient>();
            try
            {
                if (_dbContext.DeliveryClient.Where(c => c.Email == registerModel.Email).Any())
                    throw new Exception("user email exists before");
                 if (_dbContext.DeliveryClient.Where(c => c.Phone1 == registerModel.Phone1).Any())
                    throw new Exception("phone number exists before");
                registerModel.Password = HashingUtility.hashPassword(registerModel.Password);
                _dbContext.DeliveryClient.Add(registerModel);
                var rowsAffectred = _dbContext.SaveChanges();
                if (rowsAffectred > 0)
                    response.Data = registerModel;
                else
                {
                    response.HasErrors = true;
                    response.Message = "Error in Creating User";
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                response.HasErrors = true;
                response.Message = msg;
            }
            if (response.Data != null)
                response.Data.Password = "";
            return response;
        }

        public async Task<OperationResponse<DeliveryClient>> Login(LoginModel loginModel)
        {
            OperationResponse<DeliveryClient> or = new OperationResponse<DeliveryClient>();
            try
            {
                var user = await _dbContext.DeliveryClient.Where(c => c.Phone1 == loginModel.PhoneNumber && c.Password == HashingUtility.hashPassword(loginModel.password)).FirstOrDefaultAsync();
                if (user != null)
                {
                    user.Password = null;
                    or.Data = user;
                }
                else
                    throw new Exception("Incorrect phone number or password");
            }
            catch (Exception ex)
            {
                or.Message = ex.Message;
                or.HasErrors = true;
            }
            return or;
        }
        public async Task<OperationResponse<DeliveryClient>> GetUserData(decimal UserId)
        {
            OperationResponse<DeliveryClient> or = new OperationResponse<DeliveryClient>();
            try
            {
                if(UserId==0)
                    throw new Exception("Add user id");
                var user = await _dbContext.DeliveryClient.FindAsync(UserId);
                if (user != null)
                {
                    user.Password = null;
                    or.Data = user;
                }
                else
                    throw new Exception("User not found");
            }
            catch (Exception ex)
            {
                or.Message = ex.Message;
                or.HasErrors = true;
            }
            return or;
        }

    }
}
