using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using SmartHomeAutomation.API.DTOs;
using SmartHomeAutomation.API.Interfaces;
using SmartHomeAutomation.Core.Entities;
using SmartHomeAutomation.Core.Interfaces;
using System.Linq;

namespace SmartHomeAutomation.API.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<UserDto> GetUserByIdAsync(int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null || !user.IsActive)
                throw new KeyNotFoundException("Kullanıcı bulunamadı.");
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            var user = _mapper.Map<User>(createUserDto);
            user.CreatedAt = DateTime.UtcNow;
            user.IsActive = true;
            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null || !user.IsActive)
                throw new KeyNotFoundException("Kullanıcı bulunamadı.");
            _mapper.Map(updateUserDto, user);
            user.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<UserDto>(user);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null || !user.IsActive)
                return false;
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<UserDto> AuthenticateAsync(string username, string password)
        {
            var users = await _unitOfWork.Users.FindAsync(u => u.Username == username && u.IsActive);
            var user = users.FirstOrDefault();
            if (user == null || user.PasswordHash != password) // Not: Gerçek uygulamada hash kontrolü yapılmalı
                throw new UnauthorizedAccessException("Kullanıcı adı veya şifre hatalı.");
            return _mapper.Map<UserDto>(user);
        }
    }
} 