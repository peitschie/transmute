//
// Author: James Nies
// Date: 3/22/2005
// Description: The MemberAccessor class uses this interface 
//        for creating a type at runtime for accessing an individual
//        member on a target object.
//
// *** This code was written by James Nies and has been provided to you, ***
// *** free of charge, for your use.  I assume no responsibility for any ***
// *** undesired events resulting from the use of this code or the         ***
// *** information that has been provided with it .                         ***
//

using System;

namespace Transmute.Internal.FastMemberAccessor
{
    /// <summary>
    /// The IMemberAccessor interface defines a member
    /// accessor.
    /// </summary>
    public interface IMemberAccessor
    {
        /// <summary>
        /// Gets the value stored in the member for 
        /// the specified target.
        /// </summary>
        /// <param name="target">Object to retrieve
        /// the member from.</param>
        /// <returns>Member value.</returns>
        object Get(object target);

        /// <summary>
        /// Sets the value for the member of
        /// the specified target.
        /// </summary>
        /// <param name="target">Object to set the
        /// member on.</param>
        /// <param name="value">Member value.</param>
        void Set(object target, object value);

        Type MemberType { get; }
        Type ReflectedType { get; }
    }
}
