using CommonModels.User.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.User.Others
{
    public static class UserActionsAndRoles
    {
        public static readonly KeyValuePair<UserRole, UserActions[]>[] UserActionsAndRolesCollection =
        {
            new KeyValuePair<UserRole, UserActions[]>(UserRole.NotAuthenticatedUser, new UserActions[]
            {
                
            }),

            new KeyValuePair<UserRole, UserActions[]>(UserRole.Spectator, new UserActions[]
            {
                UserActions.CanLookBots,
                UserActions.CanCopyBots,
                UserActions.CanCheckDetailsBots,

                UserActions.CanLookEarnSiteTasks,
                UserActions.CanCopyEarnSiteTasks,
                UserActions.CanCheckDetailsEarnSiteTasks
            }),

            new KeyValuePair<UserRole, UserActions[]>(UserRole.Admin, new UserActions[]
            {
                UserActions.CanLookBots,
                UserActions.CanCopyBots,
                UserActions.CanCheckDetailsBots,
                UserActions.CanStopOrStartBots,
                UserActions.CanDeleteBots,
                UserActions.CanBlockBots,

                UserActions.CanLookEarnSiteTasks,
                UserActions.CanCopyEarnSiteTasks,
                UserActions.CanCheckDetailsEarnSiteTasks,
                UserActions.CanDeleteEarnSiteTasks
            })
        };
    }
}
