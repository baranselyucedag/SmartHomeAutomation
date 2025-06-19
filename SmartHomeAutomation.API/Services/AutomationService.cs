using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using SmartHomeAutomation.API.DTOs;
using SmartHomeAutomation.API.Interfaces;
using SmartHomeAutomation.Core.Entities;
using SmartHomeAutomation.Core.Interfaces;

namespace SmartHomeAutomation.API.Services
{
    public class AutomationService : IAutomationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AutomationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AutomationRuleDto>> GetAllRulesAsync()
        {
            var rules = await _unitOfWork.AutomationRules.GetAllAsync();
            return _mapper.Map<IEnumerable<AutomationRuleDto>>(rules);
        }

        public async Task<AutomationRuleDto> GetRuleByIdAsync(int id)
        {
            var rule = await _unitOfWork.AutomationRules.GetByIdAsync(id);
            return _mapper.Map<AutomationRuleDto>(rule);
        }

        public async Task<AutomationRuleDto> CreateRuleAsync(CreateAutomationRuleDto createRuleDto)
        {
            var rule = _mapper.Map<AutomationRule>(createRuleDto);
            rule.CreatedAt = DateTime.UtcNow;
            rule.IsActive = true;

            await _unitOfWork.AutomationRules.AddAsync(rule);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<AutomationRuleDto>(rule);
        }

        public async Task<AutomationRuleDto> UpdateRuleAsync(int id, UpdateAutomationRuleDto updateRuleDto)
        {
            var rule = await _unitOfWork.AutomationRules.GetByIdAsync(id);
            if (rule == null) return null;

            _mapper.Map(updateRuleDto, rule);
            rule.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.AutomationRules.UpdateAsync(rule);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<AutomationRuleDto>(rule);
        }

        public async Task<bool> DeleteRuleAsync(int id)
        {
            var rule = await _unitOfWork.AutomationRules.GetByIdAsync(id);
            if (rule == null) return false;

            // Silme yerine pasif hale getir
            rule.IsActive = false;
            rule.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.AutomationRules.UpdateAsync(rule);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ToggleRuleAsync(int id, bool isEnabled)
        {
            var rule = await _unitOfWork.AutomationRules.GetByIdAsync(id);
            if (rule == null) return false;

            rule.IsEnabled = isEnabled;
            rule.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.AutomationRules.UpdateAsync(rule);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<AutomationRuleDto>> GetUserRulesAsync(int userId)
        {
            var rules = await _unitOfWork.AutomationRules.FindAsync(r => r.UserId == userId && r.IsActive);
            return _mapper.Map<IEnumerable<AutomationRuleDto>>(rules);
        }
    }
} 