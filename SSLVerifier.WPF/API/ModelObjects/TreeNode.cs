using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SSLVerifier.API.ModelObjects {
    public class TreeNode<T> {
        public TreeNode(T value) {
            Value = value;
        }
        public TreeNode<T> this[Int32 i] => Children[i];
        public TreeNode<T> Parent { get; private set; }
        public T Value { get; }
        public ObservableCollection<TreeNode<T>> Children { get; } = new ObservableCollection<TreeNode<T>>();
        public TreeNode<T> AddChild(T value) {
            var node = new TreeNode<T>(value) { Parent = this };
            Children.Add(node);
            return node;
        }
        public TreeNode<T>[] AddChildren(params T[] values) {
            return values.Select(AddChild).ToArray();
        }
        public Boolean RemoveChild(TreeNode<T> node) {
            return Children.Remove(node);
        }
        public void Traverse(Action<T> action) {
            action(Value);
            foreach (var child in Children)
                child.Traverse(action);
        }
        public IEnumerable<T> Flatten() {
            return new[] { Value }.Union(Children.SelectMany(x => x.Flatten()));
        }
    }
}
