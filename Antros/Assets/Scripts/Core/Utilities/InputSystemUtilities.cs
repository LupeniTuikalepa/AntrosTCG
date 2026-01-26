using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Pool;

namespace ATCG.Utilities
{
    public static class InputSystemUtilities
    {
        public static void GetUsersListForActions(this IInputActionCollection collection, List<InputUser> users)
        {
            if(!collection.devices.HasValue)
                return;

            ReadOnlyArray<InputDevice> devices = collection.devices.Value;
            for (int i = 0; i < devices.Count; i++)
            {
                InputUser? user = InputUser.FindUserPairedToDevice(devices[i]);
                if (user.HasValue && !users.Contains(user.Value))
                    users.Add(user.Value);
            }
        }

        public static void GetRequiredDevices(this InputUser inputUser, List<InputDevice> devices)
        {
            if (inputUser.controlScheme.HasValue)
            {
                ReadOnlyArray<InputControlScheme.DeviceRequirement> requirements = inputUser.controlScheme.Value.deviceRequirements;
                for (var i = 0; i < requirements.Count; ++i)
                {
                    if(requirements[i].isOptional)
                        continue;

                    foreach (var device in inputUser.pairedDevices)
                    {
                        var control = InputControlPath.TryFindControl(device, requirements[i].controlPath);
                        if (control != null)
                            devices.Add(device);
                    }
                }
            }
            else
                devices.AddRange(inputUser.pairedDevices);
        }
    }
}